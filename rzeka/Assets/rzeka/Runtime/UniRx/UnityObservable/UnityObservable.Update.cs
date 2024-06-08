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

namespace Rzeka.Unirx
{
    /* ?? ---- ---- */

    public static partial class UnityObservable
    {
        /// <summary>
        /// EveryUpdate calls coroutine's yield return null timing. It is after all Update and before LateUpdate.
        /// </summary>
        public static IObservable<long> EveryUpdate()
        {
            return NewDispatcher.Instance.EveryUpdate;
        }
        
        public static IObservable<long> EveryLateUpdate()
        {
            return NewDispatcher.Instance.EveryLateUpdate;
        }
        
        public static IObservable<long> EveryFixedUpdate()
        {
            return NewDispatcher.Instance.EveryFixedUpdate;
        }
    }

    /* ---- ---- ? */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu ????? */
/* 31 May 2022 ?? */