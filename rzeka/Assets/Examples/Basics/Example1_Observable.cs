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

            /*

            http://introtorx.com/Content/v1.0.10621.0/04_CreatingObservableSequences.html#ObservableCreate

            A significant benefit that the Create method has over subjects is 
            that the sequence will be lazily evaluated. Lazy evaluation is a 
            very important part of Rx. It opens doors to other powerful features
            such as scheduling and combination of sequences that we will see later. 
            
            The delegate will only be invoked when a subscription is made.

            */
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

            using IDisposable observer = observable.Subscribe<int>(
                onNext: next => Debug.Log($"next: {next}"));

            using IDisposable yellowObserver = observable.Subscribe<int>(
                onNext: next => Debug.Log($"<color=yellow>next: {next}</color>"),
                onCompleted: () => Debug.Log($"<color=yellow>Yellow completed!</color>"));
        }

        [Button("B. Observable.Range()")]
        static void ObservableRange()
        {

            var range = Observable.Range(3, 7);

            using var rangers = range.Subscribe(
                x => Debug.Log(x), 
                () => Debug.Log(("Completed")));

        }

        [Button("C. Observable.Generate()")]
        static void ObservableGenerate()
        {
            /*

            http://introtorx.com/Content/v1.0.10621.0/04_CreatingObservableSequences.html#Unfold

            Corecursion is a function to apply to the current state to produce the 
            next state.

            An issue we may face with Observable.Create is that it can be a clumsy way to 
            produce an infinite sequence.

            The Observable.Create method also has poor support for unfolding 
            sequences using corecursion. 

            */
            int from = 0;
            int to = 3;

            var generator = Observable.Generate<int, string>(

                // an initial state
                initialState: from,

                // a predicate that defines when the sequence should terminate
                condition: x => x <= to,

                // a function to apply to the current state to produce the next state
                iterate: oldState => oldState + 1,

                // a function to transform the state to the desired output
                resultSelector: x => $"additty up from {from} to {to} is: {x}"
            );

            using var fromto = generator.Subscribe(
                x => Debug.Log(x), 
                () => Debug.Log(("Completed")));

        }

        [SerializeField, Range(0, 20)] int secretValue = 7;

        [Button("D. More interesting .Generate()")]
        void CanIGuessItIn10Attempts()
        {
            // * no, this is just a bad idea to use .Generate
            // * use it when you actually want some seequence generated
            // * in case of guessing numbers its not the sequence you actually want
           
            Func<int> throwADice = () => UnityEngine.Random.Range(0,21);

            // int attempt = 0;

            // IObservable<bool> guesser = Observable.Generate<int, bool>(
            //     initialState: 0,
            //     condition: guess => guess != secretValue && attempt < 10,
            //     iterate: _ => {

            //         int guess = throwADice.Invoke();

            //         if (guess == secretValue)
            //         {
            //             Debug.Log($"<color=yellow>Got it! Your secret value was {guess}, right?</color>");
                        
            //             return -1;
            //         }
            //         else
            //         {
            //             Debug.Log($"<color=white>Hmmm, I thought it was {guess}, but now it doesn't seem right.</color>");
                        
            //             return attempt + 1;
            //         }

            //     },
            //     resultSelector: x => x == -1
            // );

            // using var me = guesser
            //     .Where(wasGuessed => wasGuessed == true)
            //     .Subscribe(_ => Debug.Log($"<color=cyan>Yay we made it!</color>"));

        }
        /* ---- ---- ⛺ */
    }   
}
/* maria aurelia at 25 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */