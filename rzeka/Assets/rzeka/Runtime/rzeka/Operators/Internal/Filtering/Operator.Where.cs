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

    internal class WhereObservable<T> : OperatorObservableBase<T>
    {
        readonly IObservable<T> source;
        readonly Func<T, bool> predicate;
        readonly Func<T, int, bool> predicateWithIndex;


        //
        // ⛺ ─── Constructors ───────────────────────────────────────────────────
        //
        #region Constructors

        public WhereObservable(IObservable<T> source, Func<T, bool> predicate)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.predicate = predicate;
        }

        public WhereObservable(IObservable<T> source, Func<T, int, bool> predicateWithIndex)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.predicateWithIndex = predicateWithIndex;
        }

        #endregion // ---------------------------------- Constructors -------------------------

        //
        // ⛺ ─── Combining methods (process optimization) ───────────────────────────────────────────────────
        //
        #region Combining methods (process optimization)

        /// <summary>
        /// Optimize for .Where().Where()
        /// Simply combine two sequential .Where() predicates into one
        /// </summary>
        /// <param name="combinePredicate">New predicate to be &&'ed with the old one.</param>
        /// <returns></returns>
        public IObservable<T> CombinePredicate(Func<T, bool> combinePredicate)
        {
            if (this.predicate != null)
            {
                return new WhereObservable<T>(source, x => this.predicate(x) && combinePredicate(x));
            }
            else
            {
                return new WhereObservable<T>(this, combinePredicate);
            }
        }


        // Optimize for .Where().Select()
        public IObservable<TR> CombineSelector<TR>(Func<T, TR> selector)
        {
            if (this.predicate != null)
            {
                return new WhereSelectObservable<T, TR>(source, predicate, selector);
            }
            else
            {
                return new SelectObservable<T, TR>(this, selector); // can't combine
            }
        }

        #endregion // ---------------------------------- Combining methods (process optimization) -------------------------


        /// <remarks>
        /// A part of this is how is how always works for operators, operator who is an 
        /// observable creates an observer under the hood who wraps up the original 
        /// observer, it's basically a 'Decorator' pattern.
        /// 
        /// WRONG... It's important to know however that this method is only called when .Subscribe(...) call
        /// follows directly after that operator.
        /// 
        /// If there is another operator following this one, ex. .Select() after .Where()
        /// </remarks>
        protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel)
        {
            if (predicate != null)
            {
                return source.Subscribe(observer: new Where(this, observer, cancel));
            }
            else
            {
                return source.Subscribe(observer: new Where_(this, observer, cancel));
            }
        }

        class Where : OperatorObserverBase<T, T>
        {
            readonly WhereObservable<T> parent;

            public Where(WhereObservable<T> parent, IObserver<T> observer, IDisposable cancel)
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
                    UnityEngine.Debug.Log($"blerp3");

                }
                catch (Exception ex)
                {
                    try { _observer.OnError(ex); } finally { Dispose(); }
                    return;
                }

                if (isPassed)
                {
                    _observer.OnNext(value);
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

        class Where_ : OperatorObserverBase<T, T>
        {
            readonly WhereObservable<T> parent;
            int index;

            public Where_(WhereObservable<T> parent, IObserver<T> observer, IDisposable cancel)
                : base(observer, cancel)
            {
                this.parent = parent;
                this.index = 0;
            }

            public override void OnNext(T value)
            {
                var isPassed = false;
                try
                {
                    isPassed = parent.predicateWithIndex(value, index++);
                }
                catch (Exception ex)
                {
                    try { _observer.OnError(ex); } finally { Dispose(); }
                    return;
                }

                if (isPassed)
                {
                    _observer.OnNext(value);
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
/* 12 June 2022 by maeh 🌊 */