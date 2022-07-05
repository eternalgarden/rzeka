/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Rzeka;
using UnityEngine;
using System.Reactive.Subjects;

namespace Examples
{
    /* 🌊 ---- ---- */
    
    public class Strumień : MonoBehaviour
    {
        void Awake()
        {
            // -------------
            
            ISubject<StreamEvent> subject = new Subject<StreamEvent>();

            subject.OnNext(new IntStreamEvent());
            
            // -------------
        }
    }

    public class StreamEvent { }

    public class GenericStreamEvent<T> : StreamEvent { }

    public class IntStreamEvent : GenericStreamEvent<int> { }
    
    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 27 May 2022 🌊 */