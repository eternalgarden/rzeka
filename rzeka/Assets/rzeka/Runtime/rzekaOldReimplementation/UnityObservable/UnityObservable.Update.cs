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
using System.Threading;
using RzekaRiver;

namespace RzekaRiver
{
    /* 🌊 ---- ---- */

    public static partial class UnityObservable
    {
        /// <summary>
        /// EveryUpdate calls coroutine's yield return null timing. It is after all Update and before LateUpdate.
        /// </summary>
        public static IObservable<long> EveryUpdate()
        {
            return FromMicroCoroutine<long>(CycleCoreRoutineProvider, FrameCountType.Update);
        }

        public static IObservable<long> EveryFixedUpdate()
        {
            return FromMicroCoroutine<long>(CycleCoreRoutineProvider, FrameCountType.FixedUpdate);
        }

        public static IObservable<long> EveryEndOfFrame()
        {
            return FromMicroCoroutine<long>(CycleCoreRoutineProvider, FrameCountType.EndOfFrame);
        }

        static Func<IObserver<long>, CancellationToken, IEnumerator> CycleCoreRoutineProvider =>
            (observer, cancellationToken) => EveryCycleCore(observer, cancellationToken);

        /// <summary>
        /// Provides an 'inifinitely' running enumerator that invokes observer
        /// on each tick.
        /// </summary>
        /// <param name="observer">Observer of a long tick value for an internal Update 
        /// clock, you will usually discard the tick value as uninteresting.</param>
        /// <param name="cancellationToken">Cycle clock enumeration breaker.</param>
        /// <returns>Cycle loop enumerator.</returns>
        static IEnumerator EveryCycleCore(IObserver<long> observer, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested) yield break;

            long count = 0; // 0 long

            while (true)
            {
                // yield return null inside a coroutine will wait for the next frame.
                yield return null;

                if (cancellationToken.IsCancellationRequested)
                {
                    // https://stackoverflow.com/questions/231893/what-does-yield-break-do-in-c
                    // end an iterator block
                    yield break;
                }

                /*

                Aren't you afraid of an overflow?

                long contains range [0,9 223 372 036 854 775 807]

                assuming this loop ticks 60 times per second

                it would still take like 4874520144 years if I did 
                calculations correctly

                which probably I didn't

                in any case 9223372036854775807 is a big number

                */
                observer.OnNext(count++);
            }
        }

        /*
        
        TODO QUESTION
        
        TODO BELOW
        */

        // /// <summary>
        // /// EveryGameObjectUpdate calls from MainThreadDispatcher's Update.
        // /// </summary>
        // public static IObservable<long> EveryGameObjectUpdate()
        // {
        //     return MainThreadDispatcher.UpdateAsObservable().Scan(-1L, (x, y) => x + 1);
        // }

        // /// <summary>
        // /// EveryLateUpdate calls from MainThreadDispatcher's OnLateUpdate.
        // /// </summary>
        // public static IObservable<long> EveryLateUpdate()
        // {
        //     return MainThreadDispatcher.LateUpdateAsObservable().Scan(-1L, (x, y) => x + 1);
        // }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 31 May 2022 🌊 */