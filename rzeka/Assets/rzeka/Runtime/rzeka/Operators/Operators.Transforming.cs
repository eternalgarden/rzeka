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

    public static partial class Observable
    {
        // Transforming Operators
        // 
        // Operators that transform items that are emitted by an Observable.
        // The observable node type is always changed by these operators.
        // They always 'map' from a certain observable to an observable of different type.
        // Butter butter milkshake how about oats
        // 
        // https://reactivex.io/documentation/operators.html#transforming

        //
        // ⛺ ─── Select ───────────────────────────────────────────────────
        //
        #region Select

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
            var whereObservable = source as Rzeka.WhereObservable<T>;
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

        #endregion // ---------------------------------- Select -------------------------


        //
        // ⛺ ─── Select Many ───────────────────────────────────────────────────
        //
        #region Select Many

        public static IObservable<TR> SelectMany<T, TR>(this IObservable<T> source, IObservable<TR> other)
        {
            return SelectMany(source, _ => other);
        }

        public static IObservable<TR> SelectMany<T, TR>(this IObservable<T> source, Func<T, IObservable<TR>> selector)
        {
            return new SelectManyObservable<T, TR>(source, selector);
        }

        public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TResult>> selector)
        {
            return new SelectManyObservable<TSource, TResult>(source, selector);
        }

        public static IObservable<TR> SelectMany<T, TC, TR>(this IObservable<T> source, Func<T, IObservable<TC>> collectionSelector, Func<T, TC, TR> resultSelector)
        {
            return new SelectManyObservable<T, TC, TR>(source, collectionSelector, resultSelector);
        }

        public static IObservable<TResult> SelectMany<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, int, IObservable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
        {
            return new SelectManyObservable<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        }

        public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, IEnumerable<TResult>> selector)
        {
            return new SelectManyObservable<TSource, TResult>(source, selector);
        }

        public static IObservable<TResult> SelectMany<TSource, TResult>(this IObservable<TSource> source, Func<TSource, int, IEnumerable<TResult>> selector)
        {
            return new SelectManyObservable<TSource, TResult>(source, selector);
        }

        public static IObservable<TResult> SelectMany<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, IEnumerable<TCollection>> collectionSelector, Func<TSource, TCollection, TResult> resultSelector)
        {
            return new SelectManyObservable<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        }

        public static IObservable<TResult> SelectMany<TSource, TCollection, TResult>(this IObservable<TSource> source, Func<TSource, int, IEnumerable<TCollection>> collectionSelector, Func<TSource, int, TCollection, int, TResult> resultSelector)
        {
            return new SelectManyObservable<TSource, TCollection, TResult>(source, collectionSelector, resultSelector);
        }

        #endregion // ---------------------------------- Select Many -------------------------

        //
        // ⛺ ─── Cast & OfType ───────────────────────────────────────────────────
        //
        #region Cast & OfType

        /// <summary>
        /// Casts the observed type. Throws an exception if cast fails.
        /// Like .Select it's basically a 'map' operator in rx, though javascript is not
        /// a strongly typed language.
        /// https://reactivex.io/documentation/operators/map.html
        /// </summary>
        public static IObservable<TResult> Cast<TSource, TResult>(this IObservable<TSource> source)
        {
            return new CastObservable<TSource, TResult>(source);
        }

        /// <summary>
        /// Casts the observed type. Throws an exception if cast fails.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="witness">An optional parameter overload of type inference. It can be default(TResult).</param>
        /// <typeparam name="TSource">Source Type</typeparam>
        /// <typeparam name="TResult">Cast Type</typeparam>
        public static IObservable<TResult> Cast<TSource, TResult>(this IObservable<TSource> source, TResult witness)
        {
            return new CastObservable<TSource, TResult>(source);
        }

        /// <summary>
        /// Casts the observed item from TSource to TResult. Only emits items that successfuly cast, failed cast
        /// is not an error.
        /// </summary>
        public static IObservable<TResult> OfType<TSource, TResult>(this IObservable<TSource> source)
        {
            return new OfTypeObservable<TSource, TResult>(source);
        }

        /// <summary>
        /// Casts the observed item from TSource to TResult. Only emits items that successfuly cast, failed cast
        /// is not an error.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="witness">An optional parameter overload of type inference. It can be default(TResult).</param>
        /// <typeparam name="TSource">Source Type</typeparam>
        /// <typeparam name="TResult">Cast Type</typeparam>
        public static IObservable<TResult> OfType<TSource, TResult>(this IObservable<TSource> source, TResult witness)
        {
            return new OfTypeObservable<TSource, TResult>(source);
        }

        #endregion // ---------------------------------- Cast -------------------------

        //
        // ⛺ ─── Scan ───────────────────────────────────────────────────
        //
        #region Scan

        /// <summary>
        /// 'scan' of ReactiveX -> https://reactivex.io/documentation/operators/scan.html
        /// runs Accumulator Func on each next item and emits that to its own stream
        /// for an operation that on entire stream at once and emits only final accumulation result see 'Aggregate'
        /// </summary>
        /// <param name="accumulator">1. seed 2. next 3. return</param>
        public static IObservable<TSource> Scan<TSource>(this IObservable<TSource> source, Func<TSource, TSource, TSource> accumulator)
        {
            return new ScanObservable<TSource>(source, accumulator);
        }

        /// <summary>
        /// 'scan' of ReactiveX -> https://reactivex.io/documentation/operators/scan.html
        /// runs Accumulator Func on each next item and emits that to its own stream
        /// for an operation that on entire stream at once and emits only final accumulation result see 'Aggregate'
        /// </summary>
        /// <param name="accumulator">1. seed 2. next 3. return</param>
        public static IObservable<TAccumulate> Scan<TSource, TAccumulate>(this IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
        {
            return new ScanObservable<TSource, TAccumulate>(source, seed, accumulator);
        }

        #endregion // ---------------------------------- Scan -------------------------

    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 20 June 2022 🌊 */