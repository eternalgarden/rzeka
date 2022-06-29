/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using Rzeka.Operators;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    public static partial class Observable
    {
        
        //
        // ⛺ ─── Select & Where ───────────────────────────────────────────────────
        //
        #region Select & Where

        /// <summary>
        /// 'map' operator in rx
        /// https://reactivex.io/documentation/operators/map.html
        /// </summary>
        public static IObservable<TR> Select<T, TR>(this IObservable<T> source, Func<T, TR> selector)
        {
            // sometimes cause "which no ahead of time (AOT) code was generated." on IL2CPP...
            // TODO clear original comment?

            //var select = source as ISelect<T>;
            //if (select != null)
            //{
            //    return select.CombineSelector(selector);
            //}

            // optimized path
            var whereObservable = source as Rzeka.Operators.WhereObservable<T>;
            if (whereObservable != null)
            {
                return whereObservable.CombineSelector<TR>(selector);
            }

            return new SelectObservable<T, TR>(source, selector);
        }

        /// <summary>
        /// 'map' operator in rx
        /// https://reactivex.io/documentation/operators/map.html
        /// </summary>
        public static IObservable<TR> Select<T, TR>(this IObservable<T> source, Func<T, int, TR> selector)
        {
            return new SelectObservable<T, TR>(source, selector);
        }
        
        /// <summary>
        /// 'filter' operator in rx
        /// https://reactivex.io/documentation/operators/filter.html
        /// </summary>
        public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, bool> predicate)
        {
            // optimized path
            var whereObservable = source as Rzeka.Operators.WhereObservable<T>;
            if (whereObservable != null)
            {
                return whereObservable.CombinePredicate(predicate);
            }

            var selectObservable = source as Rzeka.Operators.ISelect<T>;
            if (selectObservable != null)
            {
                return selectObservable.CombinePredicate(predicate);
            }

            return new WhereObservable<T>(source, predicate);
        }

        /// <summary>
        /// 'filter' operator in rx
        /// https://reactivex.io/documentation/operators/filter.html
        /// </summary>
        public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, int, bool> predicate)
        {
            return new WhereObservable<T>(source, predicate);
        }
        
        #endregion // ---------------------------------- Select & Where -------------------------
    }
    
    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 11 June 2022 🌊 */