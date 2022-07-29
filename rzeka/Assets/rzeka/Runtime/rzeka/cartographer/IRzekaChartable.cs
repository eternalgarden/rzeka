/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using UnityEngine;

namespace Rzeka
{
    /* ---- ---- ⛺ */
    
    public abstract class RzekaCharter : ScriptableObject, IObserver<ThoughtBase>
    {
        public abstract void OnCompleted();
        public abstract void OnError(Exception error);
        public abstract void OnNext(ThoughtBase value);
        public abstract void OnObserved(WhoObserver<ThoughtBase> observer);
        public abstract void OnUnobserved(WhoObserver<ThoughtBase> observer);
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */