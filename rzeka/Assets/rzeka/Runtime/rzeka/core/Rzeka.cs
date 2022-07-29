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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    // * make it a singleton
    // * all methods static
    public class MonoRzeka : MonoBehaviour, IObservableStream
    {
        [SerializeField] RzekaCharter _cartographer;
        Subject<ThoughtBase> _rzeka;
        public Dictionary<Type, ISubject<ThoughtBase>> _strands = new Dictionary<Type, ISubject<ThoughtBase>>();

        //
        // ⛺ ─── Lifecycle ───────────────────────────────────────────────────
        //
        #region Lifecycle

        void Awake()
        {
            // -------------

            _rzeka = new Subject<ThoughtBase>();
            _cartographer = new DebugCartographer();

            var riverD = _rzeka
                .Do(
                    onNext: e => _cartographer.OnNext(e),
                    onError: err => _cartographer.OnError(err),
                    onCompleted: () => _cartographer.OnCompleted(),
                    onSubscribed: obs => _cartographer.OnObserved(obs as WhoObserver<ThoughtBase>),
                    onUnsubscribed: obs => _cartographer.OnUnobserved(obs as WhoObserver<ThoughtBase>))
                .Subscribe(
                    onNext: e =>
                    {
                        Type key = e.GetType();

                        if (_strands.ContainsKey(key) == false)
                        {
                            // * diff  
                            _strands.Add(key, new Subject<ThoughtBase>());
                        }

                        _strands[key].OnNext(e);
                    });

            // -------------
        }

        void OnApplicationQuit()
        {
            // -------------

            Debug.Log($"<color=yellow>Rzeka Quit</color>");

            _rzeka.Dispose();
            _strands = null;
            _cartographer = null;

            // -------------
        }

        #endregion // ---------------------------------- Lifecycle -------------------------


        //
        // ⛺ ─── Public Interface ───────────────────────────────────────────────────
        //
        #region Public Interface

        public void Pluck<T>(T thought) where T : ThoughtBase
        {
            _rzeka.OnNext(thought);
        }

        internal Dictionary<Type, ThoughtBB<MatterBB>> _looms = new Dictionary<Type, ThoughtBB<MatterBB>>();

        internal void Pluck<T,M>(M matter)
            where T : ThoughtBB<M>, new()
            where M : MatterBB
        {
            // _looms.Add(typeof(T), new T());
        }

        public IWeaveable<T> Weave<T, Q, A>(object who, Q question)
            where T : Thought<Q, A>
            where Q : Matter
            where A : Matter
        {
            return null;
        }

        public IWeaveable<T> Weave<T>(object who) 
            where T : ThoughtBase
        {
            // MakeLoom<T>(out ISubject<ThoughtBase> loom);

            // IWeaveable<T> weaveable = new(
            //     observable: loom.Cast<T>(),
            //     who: who,
            //     subjectDisposer: Disposable.Create(() =>
            //     {
            //         // if (subject.has)
            //         // todo only subject has hasobservers
            //         // todo replaysubject doesnt ugghhh
            //         // todo count observers manually
            //     }));

            // return weaveable;

            return null;
        }

        #endregion // ---------------------------------- Public Interface -------------------------


        //
        // ⛺ ─── Private Implementation ───────────────────────────────────────────────────
        //
        #region Private Implementation

        void MakeLoom<T>(out Loom<T> loom) where T : ThoughtBase
        {
            Type type = typeof(T);

            loom = null;

            if (_strands.ContainsKey(type))
            {
                // loom = _strands[type];
            }
            else
            {
                // * initialize new loom
                // loom = new Loom<T>();

                // if (type is ICustomSubjectProvideable<T> customSubjectProvider)
                // {
                //     loom._subject = customSubjectProvider.CreateCustomSubject<T>();
                // }
                // else
                // {
                //     loom = new Subject<ThoughtBase>();
                // }

                // // * initialize new loom side-effects
                // loom.Do(
                //     onNext: e => { },
                //     onError: err => { },
                //     onCompleted: () => { },
                //     onSubscribed: obs => { },
                //     onUnsubscribed: obs => { }
                // );

                // _strands.Add(type, loom);
            }
        }

        #endregion // ---------------------------------- Private Implementation -------------------------


    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */