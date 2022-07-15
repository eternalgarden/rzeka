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
using RzekaRiver.Unity.Operators;

namespace RzekaRiver
{
    /* 🌊 ---- ---- */
    public static partial class UnityObservable
    {
        /// <summary>
        /// MicroCoroutine is lightweight, fast coroutine dispatcher.
        /// IEnumerator supports only yield return null.
        /// </summary>
        public static IObservable<T> FromMicroCoroutine<T>(
            Func<IObserver<T>, CancellationToken, IEnumerator> routineProvider,
            FrameCountType frameCountType = FrameCountType.Update)
        {
            return new FromMicroCoroutineObservable<T>(routineProvider, frameCountType);
        }
    }
    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 31 May 2022 🌊 */