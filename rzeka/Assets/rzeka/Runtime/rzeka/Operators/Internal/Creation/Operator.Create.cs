/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    internal class CreateObservable<T> : OperatorObservableBase<T>
    {
        readonly Func<IObserver<T>, IDisposable> subscribe;

        public CreateObservable(Func<IObserver<T>, IDisposable> subscribe)
            : base(isRequiredSubscribeOnCurrentThread: true) // fail safe
        {
            this.subscribe = subscribe;
        }

        public CreateObservable(Func<IObserver<T>, IDisposable> subscribe, bool isRequiredSubscribeOnCurrentThread)
            : base(isRequiredSubscribeOnCurrentThread)
        {
            this.subscribe = subscribe;
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
        {
            // ? why specifically are we wrapping that observer 
            observer = new Create(observer, cancel);

            // run the provided observer through the Func<observer,disposable> provided
            // into the Observable.Create method.
            return subscribe(observer) ?? Disposable.Empty;
        }

        // * Nested Observer
        class Create : OperatorObserverBase<T, T>
        {
            public Create(IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
            }

            public override void OnNext(T value)
            {
                base._observer.OnNext(value);
            }

            public override void OnError(Exception error)
            {
                try { _observer.OnError(error); }
                finally { Dispose(); }
            }

            public override void OnCompleted()
            {
                try { _observer.OnCompleted(); }
                finally { Dispose(); }
            }
        }
    }

    internal class CreateObservable<T, TState> : OperatorObservableBase<T>
    {
        readonly TState state;
        readonly Func<TState, IObserver<T>, IDisposable> subscribe;

        public CreateObservable(TState state, Func<TState, IObserver<T>, IDisposable> subscribe)
            : base(true) // fail safe
        {
            this.state = state;
            this.subscribe = subscribe;
        }

        public CreateObservable(TState state, Func<TState, IObserver<T>, IDisposable> subscribe, bool isRequiredSubscribeOnCurrentThread)
            : base(isRequiredSubscribeOnCurrentThread)
        {
            this.state = state;
            this.subscribe = subscribe;
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
        {
            observer = new Create(observer, cancel);
            return subscribe(state, observer) ?? Disposable.Empty;
        }

        class Create : OperatorObserverBase<T, T>
        {
            public Create(IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
            }

            public override void OnNext(T value)
            {
                base._observer.OnNext(value);
            }

            public override void OnError(Exception error)
            {
                try { _observer.OnError(error); }
                finally { Dispose(); }
            }

            public override void OnCompleted()
            {
                try { _observer.OnCompleted(); }
                finally { Dispose(); }
            }
        }
    }

    /// <remarks>
    /// The only difference to the CreateObservable version of this operator
    /// is the exception catch clause wrapping the OnNext method call of the
    /// observer which also disposes that object before an error is thrown.
    /// </remarks>
    internal class CreateSafeObservable<T> : OperatorObservableBase<T>
    {
        readonly Func<IObserver<T>, IDisposable> subscribe;

        public CreateSafeObservable(Func<IObserver<T>, IDisposable> subscribe)
            : base(true) // fail safe
        {
            this.subscribe = subscribe;
        }

        public CreateSafeObservable(Func<IObserver<T>, IDisposable> subscribe, bool isRequiredSubscribeOnCurrentThread)
            : base(isRequiredSubscribeOnCurrentThread)
        {
            this.subscribe = subscribe;
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
        {
            observer = new CreateSafe(observer, cancel);
            return subscribe(observer) ?? Disposable.Empty;
        }

        class CreateSafe : OperatorObserverBase<T, T>
        {
            public CreateSafe(IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
            }

            public override void OnNext(T value)
            {
                try
                {
                    base._observer.OnNext(value);
                }
                catch
                {
                    Dispose(); // safe
                    throw;
                }
            }

            public override void OnError(Exception error)
            {
                try { _observer.OnError(error); }
                finally { Dispose(); }
            }

            public override void OnCompleted()
            {
                try { _observer.OnCompleted(); }
                finally { Dispose(); }
            }
        }
    }

    /* ---- ---- ⛺ */
}
/* maria aurelia at 25 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */