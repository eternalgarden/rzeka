using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    public class Stream<T> : ISpellStream
        where T : TMatter
    {
        public Type MatterType => typeof(T);
        
        /// <summary>
        /// Its as simple as that.
        /// If you want to keep a source available keep it's conjurers/sources alife.
        /// </summary>
        public bool IsAvailable => HasSources;

        public bool IsActive => HasSubject;
        
        bool HasSources => _sources.Count > 0;

        bool HasSubject => _strainSubject is not null;
        
        /// <summary>
        /// currently only keys are used, there is a common subscription disposable below
        /// this means the current approach in case of a change of number of sources is
        /// 'full replug'
        /// sources will be recombined into a new subscription on each source (dis)appearing
        /// </summary>
        List<IObservable<T>> _sources;

        IDisposable _sourcesSubscription;
        
        /// <summary>
        /// IMPORTANT 🙀
        /// By default it is a ReplaySubject with buffer size 1
        /// </summary>
        ISubject<T> _strainSubject;

        public Stream()
        {
            _sources = new();
            InitializeSubject();
        }

        public IDisposable RegisterConjurer(IObservable<T> strand)
        {
            _sources.Add(strand);
            _sourcesSubscription?.Dispose();
            _sourcesSubscription = GetCombinedSourcesStream().Subscribe(_strainSubject);
            
            var token = Disposable.Create(strand, x => { RemoveSource(x); });
            
            return token;
        }
        
        // * Notice Stream cannot force unregister those who already requested it as a source
        // * They need to do it on their own when they lose mana
        public IObservable<T> GetStream()
        {
            if (IsAvailable is false) throw new Exception("waffle ice cream");
            
            if (HasSubject is false) InitializeSubject();

            return _strainSubject.AsObservable();
        }

        public void Dispose()
        {
            _sourcesSubscription?.Dispose();
            _strainSubject = null;
        }

        void InitializeSubject()
        {
            // TODO At the moment (probably a long one) only such subject is allowed
            // A custom buffer size subject could be made depending on type attributes for example
            // Also a type that doesnt store any value and sompletely fades away on completion
            // return new Subject<T>();
            
            _strainSubject = new ReplaySubject<T>(1);
        }

        IObservable<T> GetCombinedSourcesStream()
        {
            IObservable<T> stream = null;
            
            int availableSources = _sources.Count;

            if (availableSources > 1)
            {
                // multisouce handle
                // alternatives per specific matter attribute description can be handled here
                stream = _sources.Merge();
            }
            else if (availableSources == 1)
            {
                stream = _sources.First();
            }
            else
            {
                throw new Exception("impossibiru");
            }

            return stream;
        }

        void RemoveSource(IObservable<T> source)
        {
            _sources.Remove(source);

            if (IsAvailable is false)
            {
                Dispose();
            }
        }
    }
}