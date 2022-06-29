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
using System.Linq;
using Rzeka.Operators;

namespace Rzeka.Stream
{
    /* 🌊 ---- ---- */

    public class Fillory : IObservableStream
    {
        private readonly Subject<StreamEvent> stream;
        private readonly Wellspring wellspring;

        public Fillory()
        {
            stream = new Subject<StreamEvent>();

            stream.Scan(wellspring, (wellspring, e) =>
            {
                return wellspring;
            }).Subscribe();
        }

        public void Consider<T>(T thought) where T : StreamEvent
        {
            stream.OnNext(thought);
        }

        public IQbservable<T> Observe<T>(object context, out IDisposable disposable) where T : StreamEvent
        {
            IDisposable localDisposable = null;

            IObservable<T> observable = Observable.Create<T>(subscribe: observer =>
            {
                localDisposable = stream
                    .Where(next => next is T)
                    .Subscribe((IObserver<StreamEvent>)observer);
                return localDisposable;
            });

            disposable = localDisposable;

            return observable as IQbservable<T>;
        }

        class Wellspring
        {
            Dictionary<Type, StreamEvent> theLibrary;
        }

        // public StreamEvent CreateCoreEvent()
        // {
        //     StreamEvent se = new();
        // }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */