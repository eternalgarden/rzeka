/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Threading;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    /*

    TODO Enjoy how elegant and cute these little babies idea are
    TODO But rewrite them so they are even nicer like in NetRx

    A nice litle cute thing to have anonymous action
    called on Dispose() by a disposable objects that wraps it,
    possibly along with it's own state.

    ! In UniRx this was named just as 'Disposable'

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



        #endregion // ---------------------------------- Nested Types -------------------------
    }

    /* ---- ---- ⛺ */
}
/* maria aurelia at 25 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */