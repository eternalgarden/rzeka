/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    /// <remarks>
    /// A Subject that Observes one type of event and emits another as an Observable.
    /// Currently used only by an AnonymousSubject
    /// </remarks>
    public interface ISubject<TSource, TResult> : IObserver<TSource>, IObservable<TResult>
    {
    }


    /// <remarks>
    /// Subject that provides an Observable of the same type of event it is an Observer of.
    /// </remarks>
    public interface ISubject<T> : ISubject<T, T>, IObserver<T>, IObservable<T>
    {
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 26 May 2022 🌊 */