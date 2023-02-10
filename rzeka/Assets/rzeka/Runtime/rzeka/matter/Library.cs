using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using UnityEngine;

[assembly:InternalsVisibleTo("com.rzeka.tests.playmode")]

namespace Rzeka
{
    // TODO wouldn't it be a good idea to push entire library and eris as internal classes in the end?
    public class Library
    {
        Dictionary<Type, ISpellStream> _streams;

        internal int StreamsCount => _streams.Count;
        internal IEnumerator<KeyValuePair<Type, ISpellStream>> StreamEnumerator => _streams.GetEnumerator();

        public Library()
        {
            _streams = new();
        }

        public bool DoesStreamExist<T>() where T : TMatter
        {
            Type key = typeof(T);
            return DoesStreamExist(key);
        }

        public bool DoesStreamExist(Type key)
        {
            return _streams.ContainsKey(key);
        }

        public bool IsStreamAvailable<T>() where T : TMatter
        {
            Type key = typeof(T);
            return IsStreamAvailable(key);
        }
        
        public bool IsStreamAvailable(Type key)
        {
            return DoesStreamExist(key) && _streams[key].IsAvailable;
        }

        public IObservable<T> GetConjurer<T>() where T : TMatter
        {
            Type key = typeof(T);
            Stream<T> stream = _streams[key] as Stream<T>;
            Debug.Assert(stream != null, nameof(stream) + " != null");
            IObservable<T> conjurer = stream.GetStream();
            return conjurer;
        }
        
        public IDisposable RegisterConjurer<T>(IObservable<T> strand)
            where T : TMatter
        {
            Type key = typeof(T);

            Stream<T> stream;
            if (_streams.ContainsKey(key) is false)
            {
                stream = new();
                _streams[key] = stream;
            }
            else
            {
                stream = _streams[key] as Stream<T>;
            }

            Debug.Assert(stream != null, nameof(stream) + " != null");
            
            IDisposable registrationToken = stream.RegisterConjurer(strand);

            return Disposable.Create(() =>
            {
                
                registrationToken.Dispose();

                if (stream.IsAvailable is false)
                {
                    // folding, is it necessary?
                    _streams.Remove(key);
                }
            });
        }
    }
}