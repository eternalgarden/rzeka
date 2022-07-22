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
using System.Reactive.Disposables;

namespace RzekaRiver
{

    public interface IObservableStream
    {
        IWeaveable<T> Weave<T>(object who) where T : ThoughtBase;
        void Pluck<T>(T thought) where T : ThoughtBase;
    }

    public interface IObservableStreamProposals : IObservableStream
    {
        // * reverse statement if u want 'using' scope
        ThoughtBase CreateCoreEvent();
        void Promise<T, TR>(Func<IObservable<T>, IObservable<TR>> promise, object context) where T : ThoughtBase;
        IDisposable Observe<T1, T2>(Action<IObservable<T1>, IObservable<T2>> thought, object context) where T1 : ThoughtBase where T2 : ThoughtBase;
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */