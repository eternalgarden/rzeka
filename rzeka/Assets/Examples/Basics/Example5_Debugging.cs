/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UnityEngine;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    public class Example5_Debugging : MonoBehaviour
    {
        IDisposable _everyUpdate;

        void Start()
        {
            // -------------

            _everyUpdate = UnityObservable
                .EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.A))
                .Select(_ =>
                {
                    Debug.Log($"<color=cyan>mm2</color>");
                    

                    int rand = UnityEngine.Random.Range(0, 4);

                    if (rand == 3)
                    {
                        // * notice after an exception is thrown the entire process in stopped
                        // TODO specify what that means, are all observers discarded
                        throw new System.Exception("Threw a 3");
                    }

                    return rand;
                })
                .Subscribe(
                onNext: value =>
                {
                    Debug.Log($"{value}");
                },
                //* don't implement onError unless you know what you are doing
                //* otherwise it might get less intuitive for you to debug
                onError: error =>
                {
                    Debug.Log($"<color=cyan>{error.Message}</color>");

                    DisposableWhat();
                },
                onCompleted: () =>
                {
                    Debug.Log($"<color=yellow>Completed!</color>");
                });

            // -------------
        }

        void DisposableWhat()
        {
            SingleAssignmentDisposable cancellationDisposable = _everyUpdate as SingleAssignmentDisposable;

            Debug.Log($"<color=yellow>{cancellationDisposable.IsDisposed}</color>");
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.D))
            {
                SingleAssignmentDisposable cancellationDisposable = _everyUpdate as SingleAssignmentDisposable;

                Debug.Log($"<color=yellow>Was disposed? {cancellationDisposable.IsDisposed}</color>");
            }
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 15 June 2022 🌊 */