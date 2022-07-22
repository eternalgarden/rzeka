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
using System.Reactive.Subjects;

namespace RzekaRiver
{
    internal interface BB { } // Matter
    internal interface X<out T> where T : BB // ThoughtBase
    {

    }

    internal class SomeMatter : BB { }
    internal class SomeThought : X<SomeMatter> { }

    internal class Loom<T> where T : ThoughtBase, IDisposable
        {
            ISubject<ThoughtBase> _subject;
            List<WhoObserver<T>> _observers;

            public void Dispose()
            {

            }

            public void SaveObserver(WhoObserver<T> newObserver)
            {
                _observers.Add(newObserver);
            }

            public void RemoveObserver(WhoObserver<T> bserver)
            {
                // the problem is how to know if it's one of the observers inside
                // new who observers are created on subscribe call
            }
        }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */