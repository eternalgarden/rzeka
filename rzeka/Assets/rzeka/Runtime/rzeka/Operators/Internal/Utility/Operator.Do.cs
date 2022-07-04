/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using UnityEngine;

namespace Rzeka
{
    /* 🌊 ---- ---- */


    internal class DoObservable<T> : OperatorObservableBase<T>
    {
        readonly IObservable<T> _source;
        readonly Action<T> _onNext;
        readonly Action<Exception> _onError;
        readonly Action _onCompleted;
        readonly Action<IObserver<T>> _onSubscribed;
        readonly Action<IObserver<T>> _onUnsubscribed;

        public DoObservable(IObservable<T> source)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this._source = source;
        }

        public DoObservable(
            IObservable<T> source,
            Action<T> onNext = null,
            Action<Exception> onError = null,
            Action onCompleted = null,
            Action<IObserver<T>> onSubscribed = null,
            Action<IObserver<T>> onUnubscribed = null)
            : this(source)
        {
            _onNext = onNext;
            _onError = onError;
            _onCompleted = onCompleted;
            _onSubscribed = onSubscribed;
            _onUnsubscribed = onUnubscribed;
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
        {
            IDisposable doDisposable = new Do(this, observer, cancel).Run();

            if (_onSubscribed != null)
            {
                Debug.Log($"meh");
                
                _onSubscribed.Invoke(observer);
            }

            if (_onUnsubscribed != null)
            {
                doDisposable = StableCompositeDisposable.Create(
                    doDisposable,
                    new AnonymousDisposable(() =>
                    {
                        _onUnsubscribed.Invoke(observer);
                    })
                );
            }

            return doDisposable;
        }

        class Do : OperatorObserverBase<T, T>
        {
            readonly DoObservable<T> _doObservable;

            public Do(DoObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
                _doObservable = parent;
            }

            public IDisposable Run()
            {
                return _doObservable._source.Subscribe(this);
            }

            public override void OnNext(T value)
            {
                // TODO correct this with respect to rxnet
                if (_doObservable._onNext != null)
                {
                    try
                    {
                        _doObservable._onNext(value);
                    }
                    catch (Exception ex)
                    {
                        try { _observer.OnError(ex); } finally { Dispose(); };
                        return;
                    }
                }

                _observer.OnNext(value);
            }

            public override void OnError(Exception error)
            {
                if (_doObservable._onError != null)
                {
                    try
                    {
                        _doObservable._onError(error);
                    }
                    catch (Exception ex)
                    {
                        try { _observer.OnError(ex); } finally { Dispose(); };
                        return;
                    }
                }

                try { _observer.OnError(error); } finally { Dispose(); };
            }

            public override void OnCompleted()
            {
                if (_doObservable._onCompleted != null)
                {
                    try
                    {
                        _doObservable._onCompleted();
                    }
                    catch (Exception ex)
                    {
                        base._observer.OnError(ex);
                        Dispose();
                        return;
                    }
                }

                try { _observer.OnCompleted(); } finally { Dispose(); };
            }
        }
    }

    internal class DoMiddlewareObservable<T> : OperatorObservableBase<T>
    {
        readonly IObservable<T> source;
        readonly IObserver<T> observer;

        public DoMiddlewareObservable(IObservable<T> source, IObserver<T> observer)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.observer = observer;
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
        {
            return new Do(this, observer, cancel).Run();
        }

        class Do : OperatorObserverBase<T, T>
        {
            readonly DoMiddlewareObservable<T> parent;

            public Do(DoMiddlewareObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                return parent.source.Subscribe(this);
            }

            public override void OnNext(T value)
            {
                try
                {
                    parent.observer.OnNext(value);
                }
                catch (Exception ex)
                {
                    try { _observer.OnError(ex); }
                    finally { Dispose(); }
                    return;
                }

                _observer.OnNext(value);
            }

            public override void OnError(Exception error)
            {
                try
                {
                    parent.observer.OnError(error);
                }
                catch (Exception ex)
                {
                    try { _observer.OnError(ex); }
                    finally { Dispose(); }
                    return;
                }

                try { _observer.OnError(error); }
                finally { Dispose(); }
            }

            public override void OnCompleted()
            {
                try
                {
                    parent.observer.OnCompleted();
                }
                catch (Exception ex)
                {
                    try { _observer.OnError(ex); }
                    finally { Dispose(); }
                    return;
                }

                try { _observer.OnCompleted(); }
                finally { Dispose(); }
            }
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 29 June 2022 🌊 */