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
using UnityEngine;

namespace RzekaRiver
{
    /* 🌊 ---- ---- */

    public static class RzekaExtensions
    {
        internal static IObserver<T> CreateWhoObserver<T>(Action<T> onNext, Action<Exception> onError, Action onCompleted, object who)
        {
            return new RzekaObserver<T>(onNext, onError, onCompleted, who);
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
        [SerializeField] RzekaCharter _cartographer;
        Subject<StreamEvent> _rzeka = new Subject<StreamEvent>();
        Dictionary<Type, ISubject<StreamEvent>> _strands = new Dictionary<Type, ISubject<StreamEvent>>();

        static Rzeka _instance;

        static Rzeka Instance
        {
            get
            {
                if (_instance is null)
                {
                    Debug.LogError($"TOO EARLY, AWAKE DIDN'T GO THROUGH.");
                    return null;
                }

                return _instance;
            }
        }
        
        //
        // ⛺ ─── Lifecycle ───────────────────────────────────────────────────
        //
        #region Lifecycle
        
        void Awake()
        {
            // -------------
            
            _instance = this;

            _cartographer = new DebugCartographer();

            var riverD = Instance._rzeka
                .Do(
                    onNext: e => _cartographer.OnNext(e),
                    onError: err => _cartographer.OnError(err),
                    onCompleted: () => _cartographer.OnCompleted(),
                    onSubscribed: obs => _cartographer.OnObserved(obs as RzekaObserver<StreamEvent>),
                    onUnsubscribed: obs => _cartographer.OnUnobserved(obs as RzekaObserver<StreamEvent>))
                .Observe(
                    onNext: e =>
                    {
                        Type key = e.GetType();

                        if (_strands.ContainsKey(key) == false)
                        {
                            // * diff  
                            _strands.Add(key, new Subject<StreamEvent>());
                        }

                        _strands[key].OnNext(e);
                    },
                    who: this);

            // -------------
        }

        void OnApplicationQuit()
        {
            // -------------
            
            Debug.Log($"<color=yellow>Rzeka Quit</color>");
            
            _rzeka.Dispose();
            _instance =  null;
            _strands = null;
            _cartographer = null;

            // -------------
        }
        
        #endregion // ---------------------------------- Lifecycle -------------------------

        
        //
        // ⛺ ─── Public Interface ───────────────────────────────────────────────────
        //
        #region Public Interface
        
        public static void Pluck<T>(T thought) where T : StreamEvent
        {
            Instance._rzeka.OnNext(thought);
        }

        // * rename to get strand or sth like that
        public static IObservable<T> Strand<T>() where T : StreamEvent
        {
            MakeStrand<T>(out ISubject<StreamEvent> strand);

            return strand.Cast<T>();
        }
        
        #endregion // ---------------------------------- Public Interface -------------------------


        //
        // ⛺ ─── Private Implementation ───────────────────────────────────────────────────
        //
        #region Private Implementation
        
        static void MakeStrand<T>(out ISubject<StreamEvent> strand) where T : StreamEvent
        {
            Type type = typeof(T);

            if (Instance._strands.ContainsKey(type))
            {
                strand = Instance._strands[type];
            }
            else
            {
                object[] attributes = type.GetCustomAttributes(inherit: false);
                strand = new Subject<StreamEvent>();
                // strand = new ReplaySubject<StreamEvent>()
                Instance._strands.Add(type, strand);
            }
        }
        
        #endregion // ---------------------------------- Private Implementation -------------------------
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */