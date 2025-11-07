using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using UnityEngine;

namespace Rzeka
{
    internal class InfiniteLoopException : Exception
    {
    }

    public class Stream<T> : ISpellStream
        where T : TMatter
    {
        const int MAX_TICK_COUNT_PER_SECOND = 100;
    
        public Type MatterType => typeof(T);
        
        IDisposable _sourcesSubscription;
        readonly ISubject<T> _subject;
        readonly IObserver<T> _subjectFeeder;

        int _secondsCount = 0;
        bool _isOverheat;
        bool _isHighVelocity;

        DateTimeOffset _overheatStartTime;

        public Stream()
        {
            _subject = CreateSubject();

            long previousStamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();

            _subjectFeeder = Observer.Create<T>(
                onNext: next =>
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
                        IsInfiniteLoopDetected(previousStamp, newStamp);
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
                        previousStamp = newStamp;
                    }

                    if (typeof(T).Name.Contains("EditedNoteUpdated"))
                    {
                        // Debug.Log($"<color=green>whatt</color>");
                    }

                    _subject.OnNext(next);
                })
                .NotifyOn(Scheduler.CurrentThread);
        }

        void IsInfiniteLoopDetected(long previousStamp, long newStamp)
        {
            if (_isHighVelocity) return;

            if (newStamp - previousStamp < 1000)
            {
                // Debug.Log($"<color=yellow>ere</color>");
                _secondsCount++;

                if (_secondsCount > MAX_TICK_COUNT_PER_SECOND)
                {
                    throw new InfiniteLoopException();
                }
            }
            else
            {
                _secondsCount = 0;
            }
        }

        ISubject<T> CreateSubject()
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
                subject = new ReplaySubject<T>(1, Scheduler.CurrentThread);
            }
            else if (attrs.Any(attr => attr is HasBufferAttribute))
            {
                var hasBufferAttribute = attrs
                    .First(attr => attr is HasBufferAttribute) as HasBufferAttribute;

                Debug.Assert(hasBufferAttribute != null, nameof(hasBufferAttribute) + " != null");

                subject = new ReplaySubject<T>(hasBufferAttribute.Buffer, Scheduler.CurrentThread);
            }
            else
            {
                subject = new Subject<T>();
            }

            if (attrs.Any(x => x is HighVelocityAttribute)) _isHighVelocity = true;

            return subject;
        }

        IObserver<T> SourceObserver => Observer.Create<T>(
            onNext: next => _subjectFeeder.OnNext(next));

        public IDisposable RegisterConjurer(IObservable<T> conjurer)
        {
            IDisposable token = conjurer.Subscribe(SourceObserver);
            // _sources++;

            return Disposable.Create(() =>
            {
                // TODO THIS IS SHADY
                token.Dispose();
                // _sources--;
            });
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
    }
}