/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using UnityEngine;
using NaughtyAttributes;
using System.Reactive.Subjects;
using System.Reactive;

namespace RzekaRiver.Examples
{
    /* 🌊 ---- ---- */

    public class Example4_Subject : MonoBehaviour
    {
        
        [Button("B. Subject Basics")]
        static void SubjectBasics()
        {
            Debug.ClearDeveloperConsole();

            ISubject<int> subject = new Subject<int>();

            IDisposable inlineObserverToken = subject.Subscribe(onNext: i =>
            {
                Debug.Log($"<color=yellow>Inline observer: {i}</color>");
            });

            subject.OnNext(5);

            IObserver<int> customObserverTemplate = Observer.Create<int>(
                onNext: i =>
                {
                    Debug.Log($"<color=cyan>Custom Observer: {i}</color>");
                },
                onError: ex => Debug.LogError(ex.Message),
                onCompleted: () => Debug.Log($"<color=magenta>Completed!</color>")
            );

            IDisposable customObserver1 = subject.Subscribe(customObserverTemplate);

            subject.OnNext(6);

            // Notice this will be only logged by the customObserver1
            // And will stop the Subject 
            // (uncomment to try)
            // 
            // subject.OnError(new Exception("Test Exception"));

            inlineObserverToken.Dispose();

            IDisposable customObserver2 = subject.Subscribe(customObserverTemplate);

            subject.OnNext(7);

            customObserver2.Dispose();

            // This will be logged only by customObserver1
            subject.OnCompleted();

            // This will not be logged by anyone since OnCompleded has already been
            // passed.
            // TODO but no information about that possibly bug-prone situation is passed?
            subject.OnNext(1);

            
        }
    }
    
    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 15 June 2022 🌊 */