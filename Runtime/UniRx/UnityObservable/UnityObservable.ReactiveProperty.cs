using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using UnityEngine;

namespace Rzeka.Unirx
{
    // TODO Rename to inspectable reactive property
    [Serializable]
    public class InspectableReactiveProperty<T> : IObservable<T>
    {
        static readonly IEqualityComparer<T> defaultEqualityComparer = UnityEqualityComparer.GetDefault<T>();

        [SerializeField] T _value;

        IObservable<T> _observable;
        EventHandler<T> _eventHandler;

        bool wasInitialized = false;

        public T Value
        {
            get
            {
                // Leaving the monad
                return _value;
            }
            set
            {
                if (wasInitialized is false) Initialize(_value);
                _value = value;
                _eventHandler?.Invoke(this, value);
            }
        }

        void Initialize(T initialValue)
        {
            _observable = Observable
               .FromEventPattern<T>(
                   h => _eventHandler += h,
                   h => _eventHandler -= h)
               .Select(e => e.EventArgs)
               .DistinctUntilChanged(EqualityComparer)
               .Publish(initialValue)
               .RefCount();
        }

        // todo this wont work
        void OnValidate() => _eventHandler.Invoke(this, _value);

        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (wasInitialized is false) Initialize(_value);

            return _observable.Subscribe(observer);
        }

        protected virtual IEqualityComparer<T> EqualityComparer
        {
            get
            {
                return defaultEqualityComparer;
            }
        }
    }
}