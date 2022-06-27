/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.UI;

namespace Rzeka.Examples
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

        [Button("B. Stateful Observable")]
        public void StatefulObservable()
        {
            Debug.ClearDeveloperConsole();

            State state = new(5);

            IObservable<int> statefulObservable = Observable.CreateWithState<int, State>(
                state: state,
                subscribe: (state, observer) =>
                {
                    observer.OnNext(state.intState);

                    state.intState++; // incremented 'state', notice log for the second observer

                    return observer as IDisposable;
                }
            );

            IObserver<int> customObserver = Observer.Create<int>(
                onNext: i =>
                {
                    Debug.Log($"<color=cyan>Custom Observer: {i}</color>");
                },
                onError: ex => Debug.LogError(ex.Message),
                // * notice onCompleted below will not be logged since the custom observable doesnt call it at all!
                onCompleted: () => Debug.Log($"<color=magenta>Completed!</color>") 
            );

            IDisposable anonymousToken = statefulObservable.Subscribe(
                onNext: i =>
                {
                    Debug.Log($"<color=yellow>Inline: {i}</color>");
                }
            );

            anonymousToken.Dispose();

            IDisposable customSubscription = statefulObservable.Subscribe(customObserver);

            customSubscription.Dispose();



            // * remember its very easy to create memory leaks if you do it in inspector
            // * and don't dispose subscriptions
        }

        class State
        {
            public int intState;

            public State(int state)
            {
                this.intState = state;
            }
        };

        /* ---- ---- ⛺ */
    }
}
/* maria aurelia at 25 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */