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

    /*

    TODO Enjoy how elegant and cute these little babies idea are

    A nice litle cute thing to have anonymous action
    called on Dispose() by a disposable objects that wraps it,
    possibly along with it's own state.

    */

    public static class Disposable
    {
        public static readonly IDisposable Empty = EmptyDisposable.Singleton;

        public static IDisposable Create(Action disposeAction)
        {
            return new AnonymousDisposable(disposeAction);
        }

        public static IDisposable CreateWithState<TState>(
            TState state, 
            Action<TState> disposeAction)
        {
            return new AnonymousDisposable<TState>(state, disposeAction);
        }

        
        //
        // ⛺ ─── Nested Types ───────────────────────────────────────────────────
        //
        
        #region Nested Types
        
        class EmptyDisposable : IDisposable
        {
            public static EmptyDisposable Singleton = new EmptyDisposable();

            private EmptyDisposable()
            {

            }

            public void Dispose()
            {
            }
        }

        class AnonymousDisposable : IDisposable
        {
            bool isDisposed = false;
            readonly Action dispose;

            public AnonymousDisposable(Action dispose)
            {
                this.dispose = dispose;
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    dispose();
                }
            }
        }

        class AnonymousDisposable<T> : IDisposable
        {
            bool isDisposed = false;
            readonly T state;
            readonly Action<T> dispose;

            public AnonymousDisposable(T state, Action<T> dispose)
            {
                this.state = state;
                this.dispose = dispose;
            }

            public void Dispose()
            {
                if (!isDisposed)
                {
                    isDisposed = true;
                    dispose(state);
                }
            }
        }
        
        #endregion // ---------------------------------- Nested Types -------------------------
    }

    /* ---- ---- ⛺ */
}
/* maria aurelia at 25 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */