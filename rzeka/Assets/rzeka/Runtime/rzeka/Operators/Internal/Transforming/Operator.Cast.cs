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

namespace Rzeka
{
    /* 🌊 ---- ---- */

    internal class CastObservable<TSource, TResult> : OperatorObservableBase<TResult>
    {
        readonly IObservable<TSource> source;

        public CastObservable(IObservable<TSource> source)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
        }

        protected override IDisposable SubscribeCore(IObserver<TResult> observer, IDisposable cancel)
        {
            return source.Subscribe(new Cast(observer, cancel));
        }

        class Cast : OperatorObserverBase<TSource, TResult>
        {
            public Cast(IObserver<TResult> observer, IDisposable cancel)
                : base(observer, cancel)
            {
            }

            public override void OnNext(TSource value)
            {
                var castValue = default(TResult);
                try
                {
                    castValue = (TResult)(object)value;
                }
                catch (Exception ex)
                {
                    try { _observer.OnError(ex); }
                    finally { Dispose(); }
                    return;
                }

                _observer.OnNext(castValue);
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
/* 30 June 2022 🌊 */