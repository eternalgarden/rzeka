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

namespace Rzeka.Examples
{
    public class Example1 : MonoBehaviour
    {
        /* 🌊 ---- ---- */
    
        void Start()
        {
            // -------------
            
            // Observable.
            
            IObservable<int> observable = Observable.Create<int>(
                subscribe: observer => {
                    observer.OnNext(1);
                    observer.OnNext(2);
                    observer.OnNext(3);
                    observer.OnCompleted();
                    return observer as IDisposable;
                }
            );

            // This should throw an error
            observable.Subscribe<int>();

            IDisposable observer = observable.Subscribe<int>(onNext: next => {
                Debug.Log($"next: {next}");
            });

            IDisposable yellowObserver = observable.Subscribe<int>(onNext: next => {
                Debug.Log($"<color=yellow>next: {next}</color>");
            });

            // Scheduler.DefaultSchedulers.
            
            // -------------
        }
    
        /* ---- ---- ⛺ */
    }
}
/* maria aurelia at 25 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */