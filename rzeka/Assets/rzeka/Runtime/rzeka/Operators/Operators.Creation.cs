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
using Rzeka;

namespace Rzeka
{
    public static partial class Observable
    {
        /* 🌊 ---- ---- */

        //
        // ⛺ ─── Create ───────────────────────────────────────────────────
        //

        #region Create

        /// <summary>
        /// Create anonymous observable. Observer has exception durability. 
        /// This is recommended for make operator and event like generator. 
        /// </summary>
        public static IObservable<T> Create<T>(Func<IObserver<T>, IDisposable> subscribe)
        {
            if (subscribe == null) throw new ArgumentNullException("subscribe");

            return new CreateObservable<T>(subscribe);
        }

        /// <summary>
        /// Create anonymous observable. Observer has exception durability. 
        /// This is recommended for make operator and event like generator(HotObservable). 
        /// </summary>
        /// <param name="subscribe"></param>
        /// <param name="isRequiredSubscribeOnCurrentThread">As the paramer name describes itself.</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IObservable<T> Create<T>(Func<IObserver<T>, IDisposable> subscribe, bool isRequiredSubscribeOnCurrentThread)
        {
            if (subscribe == null) throw new ArgumentNullException("subscribe");

            return new CreateObservable<T>(subscribe, isRequiredSubscribeOnCurrentThread);
        }

        /// <summary>
        /// Create anonymous observable. Observer has exception durability. This is recommended for make operator and event like generator. 
        /// </summary>
        public static IObservable<T> CreateWithState<T, TState>(TState state, Func<TState, IObserver<T>, IDisposable> subscribe)
        {
            if (subscribe == null) throw new ArgumentNullException("subscribe");

            return new CreateObservable<T, TState>(state, subscribe);
        }

        /// <summary>
        /// Create anonymous observable. Observer has exception durability. This is recommended for make operator and event like generator(HotObservable). 
        /// </summary>
        public static IObservable<T> CreateWithState<T, TState>(TState state, Func<TState, IObserver<T>, IDisposable> subscribe, bool isRequiredSubscribeOnCurrentThread)
        {
            if (subscribe == null) throw new ArgumentNullException("subscribe");

            return new CreateObservable<T, TState>(state, subscribe, isRequiredSubscribeOnCurrentThread);
        }

        #endregion // ---------------------------------- Create -------------------------

        //
        // ⛺ ─── Empty ───────────────────────────────────────────────────
        //

        #region Empty

        /// <summary>
        /// Empty Observable. Returns only OnCompleted.
        /// </summary>
        public static IObservable<T> Empty<T>()
        {
            return Empty<T>(Scheduler.DefaultSchedulers.ConstantTimeOperations);
        }

        /// <summary>
        /// Empty Observable. Returns only OnCompleted. witness is for type inference.
        /// </summary>
        public static IObservable<T> Empty<T>(T witness)
        {
            return Empty<T>(Scheduler.DefaultSchedulers.ConstantTimeOperations);
        }

        /// <summary>
        /// Empty Observable. Returns only OnCompleted on specified scheduler. witness is for type inference.
        /// </summary>
        public static IObservable<T> Empty<T>(IScheduler scheduler, T witness)
        {
            return Empty<T>(scheduler);
        }

        /// <summary>
        /// Empty Observable. Returns only OnCompleted on specified scheduler.
        /// </summary>
        public static IObservable<T> Empty<T>(IScheduler scheduler)
        {
            if (scheduler == Scheduler.Immediate)
            {
                return ImmutableEmptyObservable<T>.Instance;
            }
            else
            {
                return new EmptyObservable<T>(scheduler);
            }
        }

        #endregion // ---------------------------------- Empty -------------------------

        //
        // ⛺ ─── Never ───────────────────────────────────────────────────
        //

        // #region Never

        // /// <summary>
        // /// Non-Terminating Observable. It's no returns, never finish.
        // /// </summary>
        // public static IObservable<T> Never<T>()
        // {
        //     return ImmutableNeverObservable<T>.Instance;
        // }

        // /// <summary>
        // /// Non-Terminating Observable. It's no returns, never finish. witness is for type inference.
        // /// </summary>
        // public static IObservable<T> Never<T>(T witness)
        // {
        //     return ImmutableNeverObservable<T>.Instance;
        // }

        // #endregion // ---------------------------------- Never -------------------------


        //
        // ⛺ ─── Return & ReturnUnit ───────────────────────────────────────────────────
        //

        #region Return & ReturnUnit

        /// <summary>
        /// Return single sequence Immediately.
        /// </summary>
        public static IObservable<T> Return<T>(T value)
        {
            return Return<T>(value, Scheduler.DefaultSchedulers.ConstantTimeOperations);
        }

        /// <summary>
        /// Return single sequence on specified scheduler.
        /// </summary>
        public static IObservable<T> Return<T>(T value, IScheduler scheduler)
        {
            if (scheduler == Scheduler.Immediate)
            {
                return new ImmediateReturnObservable<T>(value);
            }
            else
            {
                return new ReturnObservable<T>(value, scheduler);
            }
        }

        // /// <summary>
        // /// Return single sequence Immediately, optimized for Unit(no allocate memory).
        // /// </summary>
        // public static IObservable<Unit> Return(Unit value)
        // {
        //     return ImmutableReturnUnitObservable.Instance;
        // }

        // /// <summary>
        // /// Return single sequence Immediately, optimized for Boolean(no allocate memory).
        // /// </summary>
        // public static IObservable<bool> Return(bool value)
        // {
        //     return (value == true)
        //         ? (IObservable<bool>)ImmutableReturnTrueObservable.Instance
        //         : (IObservable<bool>)ImmutableReturnFalseObservable.Instance;
        // }

        // /// <summary>
        // /// Return single sequence Immediately, optimized for Int32.
        // /// </summary>
        // public static IObservable<Int32> Return(int value)
        // {
        //     return ImmutableReturnInt32Observable.GetInt32Observable(value);
        // }

        // /// <summary>
        // /// Same as Observable.Return(Unit.Default); but no allocate memory.
        // /// </summary>
        // public static IObservable<Unit> ReturnUnit()
        // {
        //     return ImmutableReturnUnitObservable.Instance;
        // }

        #endregion // ---------------------------------- Return & ReturnUnit -------------------------

        
        //
        // ⛺ ─── Range ───────────────────────────────────────────────────
        //
        #region Range
        
        public static IObservable<int> Range(int start, int count)
        {
            return Range(start, count, Scheduler.DefaultSchedulers.Iteration);
        }

        public static IObservable<int> Range(int start, int count, IScheduler scheduler)
        {
            return new RangeObservable(start, count, scheduler);
        }
        
        #endregion // ---------------------------------- Range -------------------------

        
        //
        // ⛺ ─── ToObservable ───────────────────────────────────────────────────
        //
        #region ToObservable
        
        public static IObservable<T> ToObservable<T>(this IEnumerable<T> source)
        {
            return ToObservable(source, Scheduler.DefaultSchedulers.Iteration);
        }

        public static IObservable<T> ToObservable<T>(this IEnumerable<T> source, IScheduler scheduler)
        {
            return new ToObservableObservable<T>(source, scheduler);
        }
        
        #endregion // ---------------------------------- ToObservable -------------------------

        /* ---- ---- ⛺ */
    }
}
/* maria aurelia at 24 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */