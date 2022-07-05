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
using System.Reactive.Disposables;
using System.Threading;
using Rzeka;

namespace Rzeka.Unity.Operators
{
    /* 🌊 ---- ---- */

    internal class FromMicroCoroutineObservable<T> : IObservable<T>
    {
        readonly Func<IObserver<T>, CancellationToken, IEnumerator> routineProvider;
        readonly FrameCountType frameCountType;

        public FromMicroCoroutineObservable(Func<IObserver<T>, CancellationToken, IEnumerator> routineProvider, FrameCountType frameCountType)
        {
            this.routineProvider = routineProvider;
            this.frameCountType = frameCountType;
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            // FromMicroCoroutine microCoroutineObserver = new(observer, cancel);
            CancellationDisposable cancellationDisposable = new();
            CancellationToken token = cancellationDisposable.Token;

            IEnumerator routine = routineProvider(observer, token);

            switch (frameCountType)
            {
                case FrameCountType.Update:
                    MainThreadDispatcher.StartUpdateMicroCoroutine(routine);
                    break;
                case FrameCountType.FixedUpdate:
                    MainThreadDispatcher.StartFixedUpdateMicroCoroutine(routine);
                    break;
                case FrameCountType.EndOfFrame:
                    MainThreadDispatcher.StartEndOfFrameMicroCoroutine(routine);
                    break;
                default:
                    throw new ArgumentException("Invalid FrameCountType:" + frameCountType);
            }

            return cancellationDisposable;
        }

        // class FromMicroCoroutine : OperatorObserverBase<T, T>
        // {
        //     public FromMicroCoroutine(IObserver<T> observer, IDisposable cancel)
        //         : base(observer, cancel)
        //     {
        //     }

        //     public override void OnNext(T value)
        //     {
        //         try
        //         {
        //             base._observer.OnNext(value);
        //         }
        //         catch
        //         {
        //             Dispose();
        //             throw;
        //         }
        //     }

        //     public override void OnError(Exception error)
        //     {
        //         try { _observer.OnError(error); }
        //         finally { Dispose(); }
        //     }

        //     public override void OnCompleted()
        //     {
        //         try { _observer.OnCompleted(); }
        //         finally { Dispose(); }
        //     }
        // }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 31 May 2022 🌊 */