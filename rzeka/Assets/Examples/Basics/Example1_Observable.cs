/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Reactive.Linq;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace RzekaRiver.Examples
{
    public class Example1_Observable : MonoBehaviour
    {
        /* 🌊 ---- ---- */

        [Button("A. Observable.Create()")]
        static void ObservableCreate()
        {
            Debug.ClearDeveloperConsole();

            IObservable<int> observable = Observable.Create<int>(
                            subscribe: observer =>
                            {
                                observer.OnNext(1);
                                observer.OnNext(2);
                                observer.OnNext(3);
                                observer.OnCompleted();

                                return observer as IDisposable;
                            }
                        );

            // This should throw an error
            observable.Subscribe<int>();

            IDisposable observer = observable.Subscribe<int>(
                onNext: next => Debug.Log($"next: {next}"));

            IDisposable yellowObserver = observable.Subscribe<int>(
                onNext: next => Debug.Log($"<color=yellow>next: {next}</color>"),
                onCompleted: () => Debug.Log($"<color=yellow>Yellow completed!</color>"));
        }

        /* ---- ---- ⛺ */
    }
}
/* maria aurelia at 25 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */