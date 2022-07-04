/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using Rzeka;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    // Optimize for .Select().Where()

    internal class SelectWhereObservable<T, TR> : OperatorObservableBase<TR>
    {
        readonly IObservable<T> source;
        readonly Func<T, TR> selector;
        readonly Func<TR, bool> predicate;

        public SelectWhereObservable(IObservable<T> source, Func<T, TR> selector, Func<TR, bool> predicate)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.selector = selector;
            this.predicate = predicate;
        }

        protected override IDisposable SubscribeCore(IObserver<TR> observer, IDisposable cancel)
        {
            return source.Subscribe(new SelectWhere(this, observer, cancel));
        }

        class SelectWhere : OperatorObserverBase<T, TR>
        {
            readonly SelectWhereObservable<T, TR> parent;

            public SelectWhere(SelectWhereObservable<T, TR> parent, IObserver<TR> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public override void OnNext(T value)
            {
                var v = default(TR);
                try
                {
                    v = parent.selector(value);
                }
                catch (Exception ex)
                {
                    try { _observer.OnError(ex); } finally { Dispose(); }
                    return;
                }

                var isPassed = false;
                try
                {
                    isPassed = parent.predicate(v);
                }
                catch (Exception ex)
                {
                    try { _observer.OnError(ex); } finally { Dispose(); }
                    return;
                }

                if (isPassed)
                {
                    _observer.OnNext(v);
                }
            }

            public override void OnError(Exception error)
            {
                try { _observer.OnError(error); } finally { Dispose(); }
            }

            public override void OnCompleted()
            {
                try { _observer.OnCompleted(); } finally { Dispose(); }
            }
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 12 June 2022 🌊 */