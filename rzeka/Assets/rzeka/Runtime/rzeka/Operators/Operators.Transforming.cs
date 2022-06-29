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
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 20 June 2022 🌊 */