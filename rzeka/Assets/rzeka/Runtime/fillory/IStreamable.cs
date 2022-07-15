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
using System.Reactive.Subjects;

namespace RzekaRiver
{
    /* 🌊 ---- ---- */

    
    public abstract class StreamPromise<T> : StreamEvent where T : Gift
    {
        IObservableStream stream;
        IDisposable promiseBreaker;
        ISubject<PromiseResolution<T>> promise;

        // protected StreamPromise(Action<PromiseResolution<T>> onResolution, object context, params StreamEvent[] circumstances) : base(context, circumstances)
        // {
        //     // * somehow provide an instance of the stream

        //     // * Subscribe to PromiseResolution<T> events and filter them to this instance of event
        //     stream
        //         .Observe<PromiseResolution<T>>(this, out promiseBreaker)
        //         .Where(resolution => resolution[this])
        //         .Subscribe(onNext: next => promise.OnNext(next));
        // }

        public IObservable<PromiseResolution<T>> Promise => promise;

        public void Resolve(T response)
        {

        }

        public void Break()
        {
            promiseBreaker.Dispose();
        }

        private class PromiseStream : IObservable<PromiseResolution<T>>
        {
            public IDisposable Subscribe(IObserver<PromiseResolution<T>> observer)
            {

                return observer as IDisposable; //' this doesnt do much
            }
        }
    }

    public enum PromiseResolutionType { Accepted, Declined, Failed, Resolving }

    public abstract class PromiseResolution<T> : StreamEvent where T : Gift
    {
        protected abstract PromiseResolutionType ResolutionType { get; set; }

        // public PromiseResolution(object context, params StreamEvent[] causes) : base(context, causes)
        // {

        // }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 12 June 2022 🌊 */