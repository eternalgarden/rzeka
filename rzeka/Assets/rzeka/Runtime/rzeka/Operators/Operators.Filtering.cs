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

    public static partial class Observable
    {
        // Filtering Operators
        // 
        // Contrary to Transforming operateors these will never change the observed
        // type, they only control under what conditions a node in an observed stream
        // gets to be re-evoked in a new Observable created by this operator.
        // 
        // Thus a filtered observable will always emit the same or less amount 
        // of nodes than the original observable.
        // 
        // Also an important C# Rx implementation concept, operators <<always>> create
        // a new Observable stream, not only the so called Creating operators.
        // 
        // https://reactivex.io/documentation/operators.html#filtering


        //
        // ⛺ ─── Where ───────────────────────────────────────────────────
        //
        #region Where

        /// <summary>
        /// 'filter' operator in rx
        /// https://reactivex.io/documentation/operators/filter.html
        /// </summary>
        public static IObservable<T> Where<T>(this IObservable<T> source, Func<T, bool> predicate)
        {
            // optimized path
            var whereObservable = source as Rzeka.WhereObservable<T>;
            if (whereObservable != null)
            {
                return whereObservable.CombinePredicate(predicate);
            }

            var selectObservable = source as Rzeka.ISelect<T>;
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

        #endregion // ---------------------------------- Where -------------------------
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 20 June 2022 🌊 */