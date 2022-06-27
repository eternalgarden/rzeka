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

    /*
    TODO Refactor
    This is clearly an abandoned idea of neuecc since a correcponsing interface
    wasn't created for the Where operator and Combine method was just scripted
    in there with
    */
    [Obsolete]
    internal interface ISelect<TR>
    {
        IObservable<TR> CombinePredicate(Func<TR, bool> predicate);
    }

    internal class SelectObservable<T, TR> : OperatorObservableBase<TR>, ISelect<TR>
    {
        readonly IObservable<T> _source;
        readonly Func<T, TR> _selector;
        readonly Func<T, int, TR> _selectorWithIndex;

        public SelectObservable(IObservable<T> source, Func<T, TR> selector)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            _source = source;
            _selector = selector;
        }

        public SelectObservable(IObservable<T> source, Func<T, int, TR> selector)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this._source = source;
            this._selectorWithIndex = selector;
        }

        public IObservable<TR> CombinePredicate(Func<TR, bool> predicate)
        {
            if (_selector != null)
            {
                return new SelectWhereObservable<T, TR>(_source, _selector, predicate);
            }
            else
            {
                return new WhereObservable<TR>(this, predicate); // can't combine
            }
        }

        protected override IDisposable SubscribeCore(IObserver<TR> observer, IDisposable cancel)
        {
            // this if assumes either _selector or _selectorWithIndex is null
            if (_selector != null)
            {
                return _source.Subscribe(new Select(this, observer, cancel));
            }
            else
            {
                return _source.Subscribe(new Select_(this, observer, cancel));
            }
        }

        class Select : OperatorObserverBase<T, TR>
        {
            readonly SelectObservable<T, TR> parent;

            public Select(SelectObservable<T, TR> parent, IObserver<TR> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
            }

            public override void OnNext(T value)
            {
                var v = default(TR);
                try
                {
                    v = parent._selector(value);
                }
                catch (Exception ex)
                {
                    // TODO this is an insufficient error handling method, 
                    // TODO it is not specific enough about the place it happened
                    // TODO or is it?
                    try { observer.OnError(ex); } finally { Dispose(); }
                    return;
                }

                observer.OnNext(v);
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

        // with Index
        class Select_ : OperatorObserverBase<T, TR>
        {
            readonly SelectObservable<T, TR> parent;
            int index;

            public Select_(SelectObservable<T, TR> parent, IObserver<TR> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
                this.index = 0;
            }

            public override void OnNext(T value)
            {
                var v = default(TR);
                try
                {
                    v = parent._selectorWithIndex(value, index++);
                }
                catch (Exception ex)
                {
                    try { observer.OnError(ex); } finally { Dispose(); }
                    return;
                }

                observer.OnNext(v);
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
/* 11 June 2022 🌊 */