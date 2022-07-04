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

    internal class OfTypeObservable<TSource, TResult> : OperatorObservableBase<TResult>
    {
        readonly IObservable<TSource> _source;

        public OfTypeObservable(IObservable<TSource> source)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            _source = source;
        }

        protected override IDisposable SubscribeCore(IObserver<TResult> observer, IDisposable cancel)
        {
            return _source.Subscribe(new OfType(observer, cancel));
        }

        class OfType : OperatorObserverBase<TSource, TResult>
        {
            public OfType(IObserver<TResult> observer, IDisposable cancel)
                : base(observer, cancel)
            {
            }

            public override void OnNext(TSource value)
            {
                if (value is TResult)
                {
                    var castValue = (TResult)(object)value;
                    _observer.OnNext(castValue);
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
/* 01 July 2022 🌊 */