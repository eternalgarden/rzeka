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

namespace Rzeka
{
    // internal interface MatterBB { } // Matter
    // internal interface ThoughtBB<out T> : IObservable<T>
    //     where T : MatterBB // ThoughtBase
    // {
    //     IDisposable Subscribe(IObserver<T> observer)
    //     {
    //         throw new NotImplementedException();
    //     }
    // }

    // internal class SomeMatter : MatterBB { }
    // internal class SomeThought : ThoughtBB<SomeMatter>
    // {
    //     public IDisposable Subscribe(IObserver<SomeMatter> observer)
    //     {
    //         return (this as ThoughtBB<SomeMatter>).Subscribe(observer);
    //     }
    // }
    
    internal abstract class MatterBB { } // Matter
    internal abstract class ThoughtBaseBB { }
    internal abstract class ThoughtBB<T> : ThoughtBaseBB, IObservable<T>
        where T : MatterBB // ThoughtBase
    {
        public virtual IDisposable Subscribe(IObserver<T> observer)
        {
            // WhoObserver<T> whoObserver = new(observer, who);
            throw new NotImplementedException();
        }
    }

    internal class RzekaBB
    {
        public void Pluck<T,M>(M matter)
            where T : ThoughtBB<M>, new()
            where M : MatterBB
        {
            // _looms.Add(typeof(T), new T());
        }

        public IObservable<M> Weave<T,M>(object who) 
            where T : ThoughtBB<M>
            where M : MatterBB
        {
            return null;
        }
    }

    internal class SomeMatter : MatterBB { }
    internal class SomeThought : ThoughtBB<SomeMatter>
    {
        
    }

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