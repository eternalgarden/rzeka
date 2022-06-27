/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using Rzeka.Utils;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    public sealed class Subject<T> : ISubject<T>, IDisposable, IOptimizedObservable<T>
    {
        object _observerLock = new object();
        
        /// <remarks>
        /// There is no way to "un-stop" a subject. Once OnError or OnCompleted
        /// was thrown it gets marked as 'true' and there is no way to "re-up" it. 
        /// Dispose it and recreate.
        /// </remarks>
        bool _isStopped;
        bool _isDisposed;
        Exception _lastError;
        IObserver<T> _outObserver = EmptyObserver<T>.Instance;

        public bool HasObservers
        {
            get
            {
                return !(_outObserver is EmptyObserver<T>) && !_isStopped && !_isDisposed;
            }
        }

        //
        // ⛺ ─── IObserver ───────────────────────────────────────────────────
        //
        #region IObserver

        public void OnNext(T value)
        {
            _outObserver.OnNext(value);
        }

        public void OnCompleted()
        {
            IObserver<T> old;

            lock (_observerLock)
            {
                ThrowIfDisposed();

                if (_isStopped) return;

                old = _outObserver;
                _outObserver = EmptyObserver<T>.Instance;
                _isStopped = true;
            }

            old.OnCompleted();
        }

        public void OnError(Exception error)
        {
            if (error == null) throw new ArgumentNullException("error");

            IObserver<T> old;

            lock (_observerLock)
            {
                ThrowIfDisposed();
                if (_isStopped) return;

                old = _outObserver;
                _outObserver = EmptyObserver<T>.Instance;
                _isStopped = true;
                _lastError = error;
            }

            old.OnError(error);
        }

        #endregion // ---------------------------------- IObserver -------------------------

        //
        // ⛺ ─── IObservable ───────────────────────────────────────────────────
        //
        #region IObservable

        /// <summary>
        /// Implementation of IObserver interface. Allows Observers to subscribe 
        /// to the Subject. If the Subject stopped it's operation before the new
        /// Observer subscribed, the new Observer will still receive Subject's 
        /// OnCompleted or OnError call.
        /// </summary>
        /// <param name="observer">A new Subject's Observer to register.</param>
        /// <returns>If the Subject isn't stopped yet (either through OnCompleted
        /// or OnError call) it will return a Disposable that works as an Unsubscribe
        /// token. If the Subject already stopped it's operation before the new Observer
        /// registered, it will return Disposable.Empty</returns>
        /// <remarks>
        /// If Subject was already disposed before the subscription attempt was made,
        /// an ObjectDisposedException exception will be thrown.
        /// </remarks>
        public IDisposable Subscribe(IObserver<T> observer)
        {
            if (observer == null) throw new ArgumentNullException("observer");

            var ex = default(Exception);

            lock (_observerLock)
            {
                ThrowIfDisposed();

                if (!_isStopped)
                {
                    var listObserver = _outObserver as ListObserver<T>;

                    if (listObserver != null)
                    {
                        _outObserver = listObserver.Add(observer);
                    }
                    else
                    {
                        var current = _outObserver;

                        if (current is EmptyObserver<T>)
                        {
                            _outObserver = observer;
                        }
                        else
                        {
                            _outObserver = new ListObserver<T>(
                                observers: new ImmutableList<IObserver<T>>(
                                    data: new[] { current, observer }));
                        }
                    }

                    return new Subscription(this, observer);
                }

                ex = _lastError;
            }

            // If a new observer attempts to Subscribe to an already stopped
            // Subject then it will receive either its last error or oncompleted call
            // TODO Can't we just put that inisde the lock?
            if (ex != null)
            {
                observer.OnError(ex);
            }
            else
            {
                observer.OnCompleted();
            }

            return Disposable.Empty;
        }

        #endregion // ---------------------------------- IObservable -------------------------

        //
        // ⛺ ─── IDisposable ───────────────────────────────────────────────────
        //
        #region IDisposable

        /// <remarks>
        /// Subject only marks itself as Disposed. It's Observers need to Dispose their
        /// subscriptions in their own choosen time.
        /// </remarks>
        public void Dispose()
        {
            lock (_observerLock)
            {
                _isDisposed = true;
                _outObserver = DisposedObserver<T>.Instance;
            }
        }

        #endregion // ---------------------------------- IDisposable -------------------------

        //
        // ⛺ ─── IOptimizedObservable ───────────────────────────────────────────────────
        //

        #region IOptimizedObservable

        /// <remarks>
        /// Subject isn't required to subscribe on current thread. Always returns false.
        /// </remarks>
        public bool IsRequiredSubscribeOnCurrentThread()
        {
            return false;
        }

        #endregion // ---------------------------------- IOptimizedObservable -------------------------

        //
        // ⛺ ─── Private implementation ───────────────────────────────────────────────────
        //
        #region Private implementation

        void ThrowIfDisposed()
        {
            if (_isDisposed) throw new ObjectDisposedException("");
        }

        class Subscription : IDisposable
        {
            readonly object gate = new object();
            Subject<T> parent;
            IObserver<T> unsubscribeTarget;

            public Subscription(Subject<T> parent, IObserver<T> unsubscribeTarget)
            {
                this.parent = parent;
                this.unsubscribeTarget = unsubscribeTarget;
            }

            public void Dispose()
            {
                lock (gate)
                {
                    if (parent != null)
                    {
                        lock (parent._observerLock)
                        {
                            var listObserver = parent._outObserver as ListObserver<T>;

                            if (listObserver != null)
                            {
                                parent._outObserver = listObserver.Remove(unsubscribeTarget);
                            }
                            else
                            {
                                parent._outObserver = EmptyObserver<T>.Instance;
                            }

                            unsubscribeTarget = null;
                            parent = null;
                        }
                    }
                }
            }
        }

        #endregion // ---------------------------------- Private implementation -------------------------
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 26 May 2022 🌊 */