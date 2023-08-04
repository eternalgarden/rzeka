/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NUnit.Framework;
using Rzeka;
using Rzeka.Tests.Integration;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Rx
{
    public class Understanding_Publish
    {
        // -------------
    
        [UnityTest]
        public IEnumerator a_shared_subscription_through_publish()
        {
            var Q = new CollectibleDisposable();

            int count = 0; // ! EXPECTED 1

            IObservable<long> interval = Observable
                .Interval(TimeSpan.FromSeconds(0.1)) // 0.1
                .Do(_ => count++) // Since its is a subscription shared through publish it will be triggered only once
                .Publish()
                .RefCount();

            int receivals = 0; // * EXPECTED 2

            Q += interval.Subscribe(_ => receivals++);
            Q += interval.Subscribe(_ => receivals++);
            
            yield return new WaitForSeconds(0.15f); // 0.15 meaning one tick

            Q.Dispose();

            Assert.AreEqual((1,2), (count, receivals));
        }

        [UnityTest]
        public IEnumerator b_without_publish_side_effects_triggers_twice()
        {
            var Q = new CollectibleDisposable();

            int count = 0; // ! EXPECTED 2

            IObservable<long> interval = Observable
                .Interval(TimeSpan.FromSeconds(0.1)) // 0.1
                .Do(_ => count++); 
                // .Publish() Everything else is the same as above
                // .RefCount();

            int receivals = 0; // * EXPECTED 2

            Q += interval.Subscribe(_ => receivals++);
            Q += interval.Subscribe(_ => receivals++);
            
            yield return new WaitForSeconds(0.15f); // 0.15 meaning one tick

            Q.Dispose();

            Assert.AreEqual((2,2), (count, receivals));
        }

        [UnityTest]
        public IEnumerator c_weird_behaviour_of_publish_selector_overload_not_sure_of_its_use()
        {
            var Q = new CollectibleDisposable();

            int doTriggerCount = 0;

            IObservable<string> rangoToMeow = Observable
                .Range(0,2) // * thus i would expect two Do triggers
                .Do(_ => doTriggerCount++)
                .Publish<int, string>(selector: range => range
                    .Select(_ => "meow")
                    ); // Notice this .Publish overload doesnt return ConnectableObservable

            int receivals = 0;
            Q += rangoToMeow.Subscribe(_ => receivals++); // two observers
            Q += rangoToMeow.Subscribe(_ => receivals++);
            
            yield return null;

            Q.Dispose();

            // TODO however we are getting four of them
            Assert.AreEqual((4,4), (doTriggerCount, receivals));
        }
    
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 30 November 2022 🌊 */