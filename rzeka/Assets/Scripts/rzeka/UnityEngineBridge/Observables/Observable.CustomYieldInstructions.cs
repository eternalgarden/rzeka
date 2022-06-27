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
using System.Collections.Generic;

namespace Rzeka.Unity
{
    /* 🌊 ---- ---- */

    public static partial class Observable
    {
        readonly static HashSet<Type> YieldInstructionTypes = new HashSet<Type>
        {
            /*
            
            TODO QUESTION
            
            does it make sense to keep www yield instruction type
            */
//             #if UNITY_2018_3_OR_NEWER
// #pragma warning disable CS0618
// #endif
//             typeof(WWW),
//             #if UNITY_2018_3_OR_NEWER
// #pragma warning restore CS0618
// #endif
//             typeof(WaitForEndOfFrame),
//             typeof(WaitForFixedUpdate),
//             typeof(WaitForSeconds),
//             typeof(AsyncOperation),
//             typeof(Coroutine)
        };
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 31 May 2022 🌊 */