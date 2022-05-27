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
    /*

    

    */
    public static partial class IObservableExtensions
    {
        /* 🌊 ---- ---- */
    
        
        //
        // ⛺ ─── Subscribe ───────────────────────────────────────────────────
        //
        
        #region Subscribe
        
        public static IDisposable Subscribe<T>(this IObservable<T> source)
        {
            return source.Subscribe(Rzeka.Utils.ThrowObserver<T>.Instance);
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext)
        {
            return source.Subscribe(Observer.CreateSubscribeObserver(onNext, Stubs.Throw, Stubs.Nop));
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError)
        {
            return source.Subscribe(Observer.CreateSubscribeObserver(onNext, onError, Stubs.Nop));
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted)
        {
            return source.Subscribe(Observer.CreateSubscribeObserver(onNext, Stubs.Throw, onCompleted));
        }

        public static IDisposable Subscribe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            return source.Subscribe(Observer.CreateSubscribeObserver(onNext, onError, onCompleted));
        }
        
        #endregion // ---------------------------------- Subscribe -------------------------
    
        
        //
        // ⛺ ─── Subscribe With State ───────────────────────────────────────────────────
        //
        
        #region Subscribe With State
        
        // TODO Add 2 State and 2 State versions
        
        #region 1 State
        
        public static IDisposable SubscribeWithState<T, TState>(this IObservable<T> source, TState state, Action<T, TState> onNext)
        {
            return source.Subscribe(Observer.CreateSubscribeWithStateObserver(state, onNext, Stubs<TState>.Throw, Stubs<TState>.Ignore));
        }

        public static IDisposable SubscribeWithState<T, TState>(this IObservable<T> source, TState state, Action<T, TState> onNext, Action<Exception, TState> onError)
        {
            return source.Subscribe(Observer.CreateSubscribeWithStateObserver(state, onNext, onError, Stubs<TState>.Ignore));
        }

        public static IDisposable SubscribeWithState<T, TState>(this IObservable<T> source, TState state, Action<T, TState> onNext, Action<TState> onCompleted)
        {
            return source.Subscribe(Observer.CreateSubscribeWithStateObserver(state, onNext, Stubs<TState>.Throw, onCompleted));
        }

        public static IDisposable SubscribeWithState<T, TState>(this IObservable<T> source, TState state, Action<T, TState> onNext, Action<Exception, TState> onError, Action<TState> onCompleted)
        {
            return source.Subscribe(Observer.CreateSubscribeWithStateObserver(state, onNext, onError, onCompleted));
        }
        
        #endregion // 1 State
        
        #endregion // ---------------------------------- Subscribe With State -------------------------

        /* ---- ---- ⛺ */
    }
}
/* maria aurelia at 24 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */