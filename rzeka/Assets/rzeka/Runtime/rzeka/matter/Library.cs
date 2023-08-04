using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using UnityEngine;

[assembly:InternalsVisibleTo("com.rzeka.tests.playmode")]

namespace Rzeka
{
    // TODO wouldn't it be a good idea to push entire library and eris as internal classes in the end?
    public class Library
    {
        readonly Eris _eris;
        readonly Dictionary<Type, ISpellStream> _streams;

        internal int StreamsCount => _streams.Count;

        public Library(Eris eris)
        {
            _eris = eris;
            _streams = new();
        }

        public bool WasStreamCreated<T>() where T : TMatter
        {
            Type key = typeof(T);
            return WasStreamCreated(key);
        }

        public bool WasStreamCreated(Type key)
        {
            return _streams.ContainsKey(key);
        }
        
        public IDisposable RegisterConjurer<T>([NotNull] IObservable<T> strand)
            where T : TMatter
        {
            if (strand == null) throw new ArgumentNullException(nameof(strand));
            
            Stream<T> stream = GetStream<T>();

            Debug.Assert(stream != null, nameof(stream) + " != null");
            
            IDisposable registrationToken = stream.RegisterConjurer(strand);

            return registrationToken;
        }

        public IObservable<T> GetConjurer<T>() where T : TMatter
        {
            Stream<T> stream = GetStream<T>();
            IObservable<T> conjurer = stream.GetStreamAsObservable();
            return conjurer;
        }
        
        /// <summary>
        /// TODO IMPORTANT CONSIDERATION
        /// At the moment all created streams stay for the duration of application running
        /// They are never un-created only become inactive when they lack sources.
        /// They are also created BOTH by a conjurer or a stream request
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Stream<T> GetStream<T>() where T : TMatter
        {
            Type key = typeof(T);
            
            Stream<T> stream;
            
            if (_streams.ContainsKey(key) is false)
            {
                stream = new();
                _streams.Add(key, stream);
            }
            else
            {
                stream = _streams[key] as Stream<T>;
            }

            return stream;
        }
    }
}