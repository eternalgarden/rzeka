using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

        public Stream()
        {
            _subject = CreateSubject();
            _subjectFeeder = Observer.Create<T>(next => _subject.OnNext(next));
        }

        static ISubject<T> CreateSubject()
        {
            // TODO At the moment (probably a long one) only such subject is allowed
            // A custom buffer size subject could be made depending on type attributes for example
            // Also a type that doesnt store any value and sompletely fades away on completion
            // return new Subject<T>();
            ISubject<T> subject = new ReplaySubject<T>(1);
            return subject;
        }

        public IDisposable RegisterConjurer(IObservable<T> conjurer)
        {
            // contract.SourceToken = contract.SourceSubject.Subscribe(_subjectFeeder);

            AddSource(conjurer, out var newContract);
            newContract.Begin();
            IDisposable token = Disposable.Create(newContract, EndContract);
            
            return token;
        } 

        void AddSource(IObservable<T> source, out SourceContract<T> newContract)
        {
            // var sourceSubject = new Subject<T>(); //? should this be disposed at some point again?
            // var sourceSubscription = sourceSubject.Subscribe(_subjectFeeder);
            //
            // _sources.Add(source);
            // _sourceMap.Add(source, sourceSubscription);
            // RecombineSourcesSubscription();
            
            
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
        public IObservable<T> GetStream()
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
                Debug.Log($"<color=green>multiple sources</color>");
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