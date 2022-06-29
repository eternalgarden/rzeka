/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rzeka.Operators
{
    /* 🌊 ---- ---- */

    // neuecc comment:
    // Optimize for .Where().Select()

    internal class WhereSelectObservable<T, TR> : OperatorObservableBase<TR>
    {
        readonly IObservable<T> source;
        readonly Func<T, bool> predicate;
        readonly Func<T, TR> selector;

        public WhereSelectObservable(IObservable<T> source, Func<T, bool> predicate, Func<T, TR> selector)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.predicate = predicate;
            this.selector = selector;
        }

        protected override IDisposable SubscribeCore(IObserver<TR> observer, IDisposable cancel)
        {
            return source.Subscribe(new WhereSelect(this, observer, cancel));
        }

        class WhereSelect : OperatorObserverBase<T, TR>
        {
            readonly WhereSelectObservable<T, TR> parent;

            public WhereSelect(WhereSelectObservable<T, TR> parent, IObserver<TR> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public override void OnNext(T value)
            {
                var isPassed = false;
                try
                {
                    isPassed = parent.predicate(value);
                }
                catch (Exception ex)
                {
                    try { observer.OnError(ex); } finally { Dispose(); }
                    return;
                }

                if (isPassed)
                {
                    var v = default(TR);
                    try
                    {
                        Debug.Log($"<color=cyan>mm1</color>");

                        v = parent.selector(value);

                        Debug.Log($"<color=cyan>mm2</color>");
                    }
                    catch (Exception ex)
                    {
                        try { observer.OnError(ex); } finally { Dispose(); }
                        return;
                    }

                    observer.OnNext(v);
                }
            }

            public override void OnError(Exception error)
            {
                try { observer.OnError(error); } finally { Dispose(); }
            }

            public override void OnCompleted()
            {
                try { observer.OnCompleted(); } finally { Dispose(); }
            }
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 12 June 2022 🌊 */