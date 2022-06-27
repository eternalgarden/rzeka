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
using UnityEngine;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    public class Example3_BasicOperators : MonoBehaviour
    {
        [SerializeField] bool spamRegularUpdate;

        IObservable<long> _observableUpdate;
        IDisposable _updateSubscription;

        void Awake()
        {
            // -------------

            _observableUpdate = Observable.EveryUpdate();

            // -------------
        }

        void Update()
        {
            // -------------

            // -------------
        }

        // * https://docs.unity3d.com/Manual/ExecutionOrder.html
        void OnEnable()
        {
            // -------------

            _updateSubscription = _observableUpdate
                
                .Subscribe(
                    onNext: _ =>
                    {
                        if (spamRegularUpdate)
                        {
                            Debug.Log($"<color=yellow>Observable Update</color>");
                        }
                    }
            );

            // -------------
        }

        void OnDisable()
        {
            // -------------

            _updateSubscription.Dispose();

            // -------------
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 12 June 2022 🌊 */