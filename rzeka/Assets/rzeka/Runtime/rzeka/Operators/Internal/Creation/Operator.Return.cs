/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;

namespace Rzeka.Operators
{
    /* 🌊 ---- ---- */

    internal class ReturnObservable<T> : OperatorObservableBase<T>
    {
        readonly T value;
        readonly IScheduler scheduler;

        public ReturnObservable(T value, IScheduler scheduler)
            : base(scheduler == Scheduler.CurrentThread)
        {
            this.value = value;
            this.scheduler = scheduler;
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
        {
            observer = new Return(observer, cancel);

            if (scheduler == Scheduler.Immediate)
            {
                observer.OnNext(value);
                observer.OnCompleted();
                return Disposable.Empty;
            }
            else
            {
                return scheduler.Schedule(() =>
                {
                    observer.OnNext(value);
                    observer.OnCompleted();
                });
            }
        }

        class Return : OperatorObserverBase<T, T>
        {
            public Return(IObserver<T> observer, IDisposable cancel)
                : base(observer, cancel)
            {
            }

            public override void OnNext(T value)
            {
                try
                {
                    base.observer.OnNext(value);
                }
                catch
                {
                    Dispose();
                    throw;
                }
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); }
                finally { Dispose(); }
            }

            public override void OnCompleted()
            {
                try { observer.OnCompleted(); }
                finally { Dispose(); }
            }
        }
    }

    internal class ImmediateReturnObservable<T> : IObservable<T>, IOptimizedObservable<T>
    {
        readonly T value;

        public ImmediateReturnObservable(T value)
        {
            this.value = value;
        }

        public bool IsRequiredSubscribeOnCurrentThread()
        {
            return false;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observer.OnNext(value);
            observer.OnCompleted();
            return Disposable.Empty;
        }
    }
    
    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 18 June 2022 🌊 */