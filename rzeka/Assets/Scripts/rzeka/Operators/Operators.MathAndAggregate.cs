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

    public static partial class Observable
    {
        /// <summary>
        /// 'reduce' operation in ReactiveX
        /// https://reactivex.io/documentation/operators/reduce.html
        /// this is the simplest overload, all three, the seed, source and result are of the same type
        /// note: please remember aggregate will block any stream untill it executes OnCompleted
        /// </summary>
        public static IObservable<TSource> Aggregate<TSource>(this IObservable<TSource> source, Func<TSource, TSource, TSource> accumulator)
        {
            return new AggregateObservable<TSource>(source, accumulator);
        }

        /// <summary>
        /// 'reduce' operation in ReactiveX
        /// https://reactivex.io/documentation/operators/reduce.html
        /// probably most common overload, accumulator/seed works on a different type than the observed source
        /// note: please remember aggregate will block any stream untill it executes OnCompleted
        /// </summary>
        public static IObservable<TAccumulate> Aggregate<TSource, TAccumulate>(this IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator)
        {
            return new AggregateObservable<TSource, TAccumulate>(source, seed, accumulator);
        }

        /// <summary>
        /// 'reduce' operation in ReactiveX
        /// https://reactivex.io/documentation/operators/reduce.html
        /// an additional selector is performed on the result of accumulation
        /// note: please remember aggregate will block any stream untill it executes OnCompleted
        /// </summary>
        public static IObservable<TResult> Aggregate<TSource, TAccumulate, TResult>(this IObservable<TSource> source, TAccumulate seed, Func<TAccumulate, TSource, TAccumulate> accumulator, Func<TAccumulate, TResult> resultSelector)
        {
            return new AggregateObservable<TSource, TAccumulate, TResult>(source, seed, accumulator, resultSelector);
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 18 June 2022 🌊 */