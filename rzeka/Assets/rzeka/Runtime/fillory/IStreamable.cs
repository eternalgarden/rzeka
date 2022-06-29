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

namespace Rzeka.Stream
{
    /* 🌊 ---- ---- */
    
    public interface IQbservable<T> : IObservable<T>
    {
        // * to be considered for the reason of an observable having other
        // * public methods thsn subscribed
    }

    public interface IObservableStream
    {
        IQbservable<T> Observe<T>(object context, out IDisposable disposable) where T : StreamEvent;
        void Consider<T>(T thought) where T : StreamEvent;
    }

    public interface IObservableStreamProposals : IObservableStream
    {
        StreamEvent CreateCoreEvent();
        void Promise<T, TR>(Func<IObservable<T>, IObservable<TR>> promise, object context) where T : StreamEvent;
        IObservable<PromiseResolution<T>> Promise<T>(object context) where T : Gift;
        IDisposable Observe<T1, T2>(Action<IObservable<T1>, IObservable<T2>> thought, object context) where T1 : StreamEvent where T2 : StreamEvent;
    }

    [Serializable]
    public class LogGift : Gift
    {
        public string Log { get; set; }
    }

    public class LogEvent : StreamGiftEvent<LogGift>
    {
        // public LogEvent(LogGift gift, object context) : base(gift, context) { }

        public override string Description => "An thought occured to a dreamer.";
    }

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