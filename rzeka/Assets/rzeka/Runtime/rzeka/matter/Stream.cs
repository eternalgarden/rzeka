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
        readonly List<IObservable<T>> _sources = new();

        bool HasSources => _sources.Count > 0;

        IDisposable _sourcesSubscription;
        ISubject<T> _subject;

        /// <summary>
        /// IMPORTANT 🙀
        /// By default it is a ReplaySubject with buffer size 1
        /// </summary>
        ISubject<T> Subject
        {
            get { return _subject ??= CreateSubject(); }
        }

        public IDisposable RegisterConjurer(IObservable<T> conjurer)
        {
            AddSource(conjurer);

            IDisposable token = Disposable.Create(conjurer, RemoveSource);
            
            return token;
        }

        void AddSource(IObservable<T> source)
        {
            _sources.Add(source);
            RecombineSourcesSubscription();
        }

        void RemoveSource(IObservable<T> source)
        {
            _sources.Remove(source);
            RecombineSourcesSubscription();
        }

        void RecombineSourcesSubscription()
        {
            _sourcesSubscription?.Dispose();
            
            if (HasSources is false) return;
            
            IObservable <T> combinedSourcesStream = CombineSources(_sources);
            _sourcesSubscription = combinedSourcesStream.Subscribe(Subject.AsObserver());
        }

        // * Notice Stream cannot force unregister those who already requested it as a source
        // * They need to do it on their own when they lose mana
        public IObservable<T> GetStream()
        {
            return Subject.AsObservable();
        }

        public void Dispose()
        {
            _sourcesSubscription?.Dispose();
        }

        ISubject<T> CreateSubject()
        {
            // TODO At the moment (probably a long one) only such subject is allowed
            // A custom buffer size subject could be made depending on type attributes for example
            // Also a type that doesnt store any value and sompletely fades away on completion
            // return new Subject<T>();
            
            return new ReplaySubject<T>(1);
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