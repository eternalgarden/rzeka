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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Rzeka;
using Rzeka.Stream;
using UnityEngine;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    public static class RzekaExtensions
    {
        internal static IObserver<T> CreateWhoObserver<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted, object who)
        {
            return new WhoObserver<T>(onNext, onError, onCompleted, who);
        }

        // public static IDisposable Observe<T>(this IObservable<T> source, Action<T> onNext, object who)
        // {
        //     return source.Subscribe(CreateWhoObserver(onNext, Stubs.Throw, Stubs.Nop, who));
        // }

        // public static IDisposable Observe<T>(this IObservable<T> source, Action<T> onNext, Action<Exception> onError, object who)
        // {
        //     return source.Subscribe(CreateWhoObserver(onNext, onError, Stubs.Nop, who));
        // }

        // public static IDisposable Observe<T>(this IObservable<T> source, Action<T> onNext, Action onCompleted, object who)
        // {
        //     return source.Subscribe(CreateWhoObserver(onNext, Stubs.Throw, onCompleted, who));
        // }

        public static IDisposable Observe<T>(this IObservable<T> source, Action<T> onNext, object who)
        {
            return source.Subscribe(CreateWhoObserver(onNext, null, null, who));
        }
    }

    public interface IQbservable<T> : IObservable<T>
    {
        // * to be considered for the reason of an observable having other
        // * public methods thsn subscribed
    }

    public interface IObservableStream
    {
        IObservable<T> Strand<T>() where T : StreamEvent;
        void Pluck<T>(T thought) where T : StreamEvent;
    }

    public interface IObservableStreamProposals : IObservableStream
    {
        // * reverse statement if u want 'using' scope
        IDisposable Observe<T>(object context, out IQbservable<T> observable) where T : StreamEvent;

        StreamEvent CreateCoreEvent();
        void Promise<T, TR>(Func<IObservable<T>, IObservable<TR>> promise, object context) where T : StreamEvent;
        IObservable<PromiseResolution<T>> Promise<T>(object context) where T : Gift;
        IDisposable Observe<T1, T2>(Action<IObservable<T1>, IObservable<T2>> thought, object context) where T1 : StreamEvent where T2 : StreamEvent;
    }

    // * make it a singleton
    // * all methods static
    public class Rzeka : MonoBehaviour
    {

        Subject<StreamEvent> rzeka = new Subject<StreamEvent>();
        Dictionary<Type, ISubject<StreamEvent>> strands = new Dictionary<Type, ISubject<StreamEvent>>();
        IRzekaChartable<StreamEvent> cartographer;

        private static Rzeka _instance;

        public static Rzeka Instance
        {
            get
            {
                if (_instance is null)
                {
                    Debug.LogError($"TOO EARLY, AWAKE DIDN'T GO THROUGH.");
                }

                return _instance;
            }
        }

        void Awake()
        {
            // -------------
            
            _instance = this;

            cartographer = new LoggingCartographer();

            var riverD = Instance.rzeka
                .Do(
                    onNext: e => cartographer.OnNext(e),
                    onError: err => cartographer.OnError(err),
                    onCompleted: () => cartographer.OnCompleted(),
                    onSubscribed: obs => cartographer.OnSubscribed(obs),
                    onUnsubscribed: obs => cartographer.OnUnsubscribed(obs))
                .Observe(
                    onNext: e =>
                    {
                        Type key = e.GetType();

                        if (strands.ContainsKey(key) == false)
                        {
                            // * diff  
                            strands.Add(key, new Subject<StreamEvent>());
                        }

                        strands[key].OnNext(e);
                    },
                    who: this);

            // -------------
        }

        void OnApplicationQuit()
        {
            // -------------

            Debug.Log($"destroy");

            rzeka.Dispose();
            strands = null;
            cartographer = null;

            // -------------
        }

        public static void Pluck<T>(T thought) where T : StreamEvent
        {
            Instance.rzeka.OnNext(thought);
        }

        // * rename to get strand or sth like that
        public static IObservable<T> Strand<T>() where T : StreamEvent
        {
            IObservable<StreamEvent> strand = Instance.strands[typeof(T)];

            return strand.Cast<T>();
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */