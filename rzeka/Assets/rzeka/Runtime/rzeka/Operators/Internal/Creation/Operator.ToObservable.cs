/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Collections.Generic;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    internal class ToObservableObservable<T> : OperatorObservableBase<T>
    {
        readonly IEnumerable<T> source;
        readonly IScheduler scheduler;

        public ToObservableObservable(IEnumerable<T> source, IScheduler scheduler)
            : base(scheduler == Scheduler.CurrentThread)
        {
            this.source = source;
            this.scheduler = scheduler;
        }

        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
        {
            return new ToObservable(this, observer, cancel).Run();
        }

        class ToObservable : OperatorObserverBase<T, T>
        {
            readonly ToObservableObservable<T> parent;

            public ToObservable(ToObservableObservable<T> parent, IObserver<T> observer, IDisposable cancel) : base(observer, cancel)
            {
                this.parent = parent;
            }

            public IDisposable Run()
            {
                var e = default(IEnumerator<T>);
                try
                {
                    e = parent.source.GetEnumerator();
                }
                catch (Exception exception)
                {
                    OnError(exception);
                    return Disposable.Empty;
                }

                if (parent.scheduler == Scheduler.Immediate)
                {
                    while (true)
                    {
                        bool hasNext;
                        var current = default(T);
                        try
                        {
                            hasNext = e.MoveNext();
                            if (hasNext) current = e.Current;
                        }
                        catch (Exception ex)
                        {
                            e.Dispose();
                            try { _observer.OnError(ex); }
                            finally { Dispose(); }
                            break;
                        }

                        if (hasNext)
                        {
                            _observer.OnNext(current);
                        }
                        else
                        {
                            e.Dispose();
                            try { _observer.OnCompleted(); }
                            finally { Dispose(); }
                            break;
                        }
                    }

                    return Disposable.Empty;
                }

                var flag = new SingleAssignmentDisposable();
                flag.Disposable = parent.scheduler.Schedule(self =>
                {
                    if (flag.IsDisposed)
                    {
                        e.Dispose();
                        return;
                    }

                    bool hasNext;
                    var current = default(T);
                    try
                    {
                        hasNext = e.MoveNext();
                        if (hasNext) current = e.Current;
                    }
                    catch (Exception ex)
                    {
                        e.Dispose();
                        try { _observer.OnError(ex); }
                        finally { Dispose(); }
                        return;
                    }

                    if (hasNext)
                    {
                        _observer.OnNext(current);
                        self();
                    }
                    else
                    {
                        e.Dispose();
                        try { _observer.OnCompleted(); }
                        finally { Dispose(); }
                    }
                });

                return flag;
            }

            public override void OnNext(T value)
            {
                // do nothing
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
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 02 July 2022 🌊 */