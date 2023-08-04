using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using JetBrains.Annotations;
using UnityEngine;

namespace Rzeka
{
    internal class SourceContract<T> : IDisposable  where T : TMatter 
    {
        readonly IObservable<T> _source;
        readonly Subject<T> _sourceSubject;

        public Subject<T> SourceSubject => _sourceSubject;

        IDisposable _sourceToken;

        public SourceContract(IObservable<T> source)
        {
            _source = source;
            _sourceSubject = new();
        }
        
        public void Begin()
        {
            _sourceToken = _source.Subscribe(next => _sourceSubject.OnNext(next));
        }

        public void Dispose()
        {
            SourceSubject?.Dispose();
            _sourceToken?.Dispose();
        }
    }
    
    internal class InfiniteLoopException : Exception { }
    
    public class Stream<T> : ISpellStream
        where T : TMatter
    {
        
        public Type MatterType => typeof(T);
        
        /// <summary>
        /// currently only keys are used, there is a common subscription disposable below
        /// this means the current approach in case of a change of number of sources is
        /// 'full replug'
        /// sources will be recombined into a new subscription on each source (dis)appearing
        /// </summary>
        readonly List<SourceContract<T>> _contracts = new();

        bool HasSources => _contracts.Count > 0;

        IDisposable _sourcesSubscription;
        readonly ISubject<T> _subject;
        readonly IObserver<T> _subjectFeeder;

        long _previousStamp;
        int _secondsCount = 0;
        bool _isOverheat;
        DateTimeOffset _overheatStartTime;
        
        public Stream()
        {
            _subject = CreateSubject();
            
            _previousStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
            
            _subjectFeeder = Observer.Create<T>(next =>
            {
                long newStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeMilliseconds();

                if (_isOverheat)
                {
                    if (newStamp - _overheatStartTime.ToUnixTimeMilliseconds() > 1000)
                    {
                        _isOverheat = false;
                    }

                    return;
                }
                
                try
                {
                    IsInfiniteLoopDetected(_previousStamp, newStamp);
                }
                catch (InfiniteLoopException e)
                {
                    Debug.LogError($"LOOPIDY LOOP! {typeof(T)}");
                    _isOverheat = true;
                    _overheatStartTime = new DateTimeOffset(DateTime.UtcNow);
                    return;
                }
                finally
                {
                    _previousStamp = newStamp;
                }
                
                _subject.OnNext(next);
            });
        }

        void IsInfiniteLoopDetected(long previousStamp, long newStamp)
        {
            if (newStamp - previousStamp < 1000)
            {
                // Debug.Log($"<color=yellow>ere</color>");
                _secondsCount++;

                if (_secondsCount > 100)
                {

                    throw new InfiniteLoopException();
                }
            }
            else
            {
                _secondsCount = 0;
            }
        }

        static ISubject<T> CreateSubject()
        {
            // TODO At the moment (probably a long one) only such subject is allowed
            // A custom buffer size subject could be made depending on type attributes for example
            // Also a type that doesnt store any value and sompletely fades away on completion
            // return new Subject<T>();
            
            ISubject<T> subject;
            Type matterType = typeof(T);
            var attrs = matterType
                .GetCustomAttributes()
                .ToArray();
            
            if (attrs.Any(attr => attr is HasStateAttribute))
            {
                subject = new ReplaySubject<T>(1);
            }
            else if (attrs.Any(attr => attr is HasBufferAttribute))
            {
                var hasBufferAttribute = attrs
                    .First(attr => attr is HasBufferAttribute) as HasBufferAttribute;

                Debug.Assert(hasBufferAttribute != null, nameof(hasBufferAttribute) + " != null");
                
                subject = new ReplaySubject<T>(hasBufferAttribute.Buffer);
            }
            else subject = new Subject<T>();
             
            return subject;
        }

        public IDisposable RegisterConjurer(IObservable<T> conjurer)
        {
            CreateContract(conjurer, out var newContract);
            newContract.Begin();
            IDisposable token = Disposable.Create(newContract, EndContract);
            
            return token;
        } 

        void CreateContract(IObservable<T> source, out SourceContract<T> newContract)
        {
            newContract = new(source);
            _contracts.Add(newContract);
            RecombineSourcesSubscription();
        }

        void EndContract(SourceContract<T> contract)
        {
            _contracts.Remove(contract);
            contract.Dispose();
            RecombineSourcesSubscription();
        }

        void RecombineSourcesSubscription()
        {
            if (HasSources is false) return;
            
            _sourcesSubscription?.Dispose();

            IObservable<T>[] sources = _contracts
                .Select(c => c.SourceSubject.AsObservable())
                .ToArray();

            IObservable <T> combinedSourcesStream = CombineSources(sources);
            _sourcesSubscription = combinedSourcesStream.Subscribe(_subjectFeeder);
        }

        // * Notice Stream cannot force unregister those who already requested it as a source
        // * They need to do it on their own when they lose mana
        public IObservable<T> GetStreamAsObservable()
        {
            return _subject.AsObservable();
        }

        public void Dispose()
        {
            _sourcesSubscription?.Dispose();
        }

        static IObservable<T> CombineSources([NotNull] IReadOnlyCollection<IObservable<T>> sources)
        {
            if (sources == null) throw new ArgumentNullException(nameof(sources));
            
            IObservable<T> stream = null;

            int availableSources = sources.Count;

            if (availableSources > 1)
            {
                // multisouce handle
                // alternatives per specific matter attribute description can be handled here
                stream = sources.Merge();
            }
            else if (availableSources == 1)
            {
                stream = sources.First();
            }
            else
            {
                Debug.Log("impossibiru");
            }

            return stream;
        }
    }
}