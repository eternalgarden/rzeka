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
using System.Collections;
using System.Collections.Generic;
using Rzeka.Examples;
using Rzeka.Operators;
using UnityEngine;
using NaughtyAttributes;

namespace Rzeka.Examples
{
    public class Example1 : MonoBehaviour
    {
        /* 🌊 ---- ---- */
    
        void Start()
        {
            // -------------

            // -------------
        }

        [Button("Subject basics")]
        private static void NewMethod()
        {
            Debug.ClearDeveloperConsole();

            ISubject<int> subject = new Subject<int>();

            IDisposable inlineObserverToken = subject.Subscribe(onNext: i =>
            {
                Debug.Log($"<color=yellow>Inline observer: {i}</color>");
            });

            subject.OnNext(5);

            IObserver<int> customObserverTemplate = Observer.Create<int>(
                onNext: i => {
                    Debug.Log($"<color=cyan>Custom Observer: {i}</color>");
                },
                onError: ex => Debug.LogError(ex.Message),
                onCompleted: () => Debug.Log($"<color=magenta>Completed!</color>")
            );

            IDisposable customObserver1 = subject.Subscribe(customObserverTemplate);

            subject.OnNext(6);

            // Notice this will be only logged by the customObserver1
            // And will stop the Subject
            // subject.OnError(new Exception("Test Exception"));

            inlineObserverToken.Dispose();

            IDisposable customObserver2 = subject.Subscribe(customObserverTemplate);

            subject.OnNext(7);

            customObserver2.Dispose();

            // This will be logged only by customObserver1
            subject.OnCompleted();

            // This will not be logged by anyone since OnCompleded has already been
            // passed.
            subject.OnNext(1);
        }

        [Button("Observable Create")]
        private static void ObservableCreate()
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

            IDisposable observer = observable.Subscribe<int>(onNext: next =>
            {
                Debug.Log($"next: {next}");
            });

            IDisposable yellowObserver = observable.Subscribe<int>(onNext: next =>
            {
                Debug.Log($"<color=yellow>next: {next}</color>");
            });
        }

        /* ---- ---- ⛺ */
    }
}
/* maria aurelia at 25 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */