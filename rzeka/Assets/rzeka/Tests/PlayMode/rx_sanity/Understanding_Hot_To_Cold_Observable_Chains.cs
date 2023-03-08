using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Rzeka.Tests.EIntegration;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Rx
{
    public class Understanding_Hot_To_Cold_Observable_Chains : TestBase
    {
        [UnityTest]
        public IEnumerator a_Do_NOT_getting_called_on_cold_observable_without_observers()
        {
            // -------------

            int count = 0;

            var observable = Observable
                .Return(1)
                .Do(onNext: _ => count++);

            yield return null;

            // * Do will not get called since it is a cold observable and there are no observers.
            AssertEqual(0, count);

            // -------------
        }

        
        [UnityTest]
        public IEnumerator b_Do_GETS_called_on_connected_hot_observable_without_observers()
        {
            // -------------

            int count = 0;

            IConnectableObservable<int> connectableObservable = Observable
                .Return(1)
                .Do(onNext: _ => count++)
                .Publish();

            using var connection = connectableObservable.Connect();

            yield return null;

            // * Notice there are still no observers but the Do is getting called since it is a hot observable
            AssertEqual(1, count);

            // -------------
        }
        
        [UnityTest]
        public IEnumerator c_Do_transforms_HOT_observable_to_a_COLD_observable()
        {
            // -------------

            int count = 0;

            IConnectableObservable<int> connectableObservable = Observable
                .Return(1)
                .Publish();

            IObservable<int> anotherObservable = connectableObservable
                .Do(onNext: _ => count++);

            using var connection = connectableObservable.Connect();

            yield return null;

            // * In this case Do will NOT get called again because it is chained AFTER a .Multicast operator (.Publish)
            // * This effectively creates a new COLD observable that performs actions on a hot observable
            // * This will be true for most other operators (apart obviously from multicast and its variants)
            AssertEqual(0, count);

            // -------------
        }

        [UnityTest]
        public IEnumerator d_Do_transforms_HOT_observable_to_a_COLD_observable_with_late_subscriber()
        {
            // -------------

            int count = 0;

            IConnectableObservable<int> connectableObservable = Observable
                .Return(1)
                .Publish();

            IObservable<int> anotherObservable = connectableObservable
                .Do(onNext: _ => count++);

            using var connection = connectableObservable.Connect();

            using var lateObserver = anotherObservable.Subscribe(_ => count++); // neither will this

            yield return null;

            // * Do will still not get called because it subscribed too late
            AssertEqual(0, count);

            // -------------
        }

        

        [UnityTest]
        public IEnumerator e_HotToCold_stacked_observable_with_ONE_observer()
        {
            // -------------

            int count = 0;

            IConnectableObservable<int> connectableObservable = Observable
                .Return(1)
                .Replay(); // * This time with replay instead

            IObservable<int> anotherObservable = connectableObservable
                .Do(onNext: _ => count++);

            using var connection = connectableObservable.Connect();

            using var lateObserver = anotherObservable.Subscribe();

            yield return null;

            AssertEqual(1, count); // * We are getting the expected 1

            // -------------
        }

        [UnityTest]
        public IEnumerator f_HotToCold_stacked_observable_with_TWO_observers()
        {
            // -------------

            int count = 0;

            IConnectableObservable<int> connectableObservable = Observable
                .Return(1)
                .Replay(); // ! Nothing changed here, it's as in test above

            IObservable<int> anotherObservable = connectableObservable
                .Do(onNext: _ => count++);

            using var connection = connectableObservable.Connect();

            using var lateObserver1 = anotherObservable.Subscribe();
            using var lateObserver2 = anotherObservable.Subscribe(); // ! one additional observer

            yield return null;

            // * imortant
            // Since .Do was stacked after .Multicast of .Replay it will also get called twice
            // Thus if you stack a Cold operator after a Hot one 
            // And then stack some more oparators or observers
            // It is more like that .Do is PREPENDED to the following operators
            // And not appended as it seems visually in code to the hot operator
            AssertEqual(2, count); // * NOTICE NOW .DO GOT TRIGGERED TWICE NOW

            // -------------
        }

        [UnityTest]
        public IEnumerator g_the_dangers_of_such_misunderstanding()
        {
            // -------------


            IConnectableObservable<int> connectableObservable = Observable
                .Range(0,4)
                .Replay();

            int emission = 0;
            IObservable<int> anotherObservable = connectableObservable
                .Do(onNext: _ => emission++); // * you could be mistaken to think it will trigger 4 times only

            using var connection = connectableObservable.Connect();

            int receivals = 0;
            using var lateObserver1 = anotherObservable.Subscribe(_ => receivals++);
            using var lateObserver2 = anotherObservable.Subscribe(_ => receivals++);

            yield return null;

            // * But there's 8 times, exactly as many as receivals
            AssertEqual((emission: 8, receivals: 8), (emission, receivals));

            // -------------
        }

        [UnityTest]
        public IEnumerator g_HotToHot_stacked_observable_with_TWO_observers()
        {
            // -------------

            int count = 0;
            int receivedCount = 0;

            IConnectableObservable<int> connectableObservable = Observable
                .Return(1)
                .Replay();

            IConnectableObservable<int> anotherObservable = connectableObservable
                .Do(onNext: _ => count++)
                .Publish(); // * Also as multicast now

            using var connection = connectableObservable.Connect();

            using var lateObserver1 = anotherObservable.Subscribe(onNext: _ => receivedCount++);
            using var lateObserver2 = anotherObservable.Subscribe(onNext: _ => receivedCount++);

            using var anotherConnection = anotherObservable.Connect();

            yield return null;

            // * will it work
            AssertEqual((1,2), (count,receivedCount));

            // -------------
        }
    }
}