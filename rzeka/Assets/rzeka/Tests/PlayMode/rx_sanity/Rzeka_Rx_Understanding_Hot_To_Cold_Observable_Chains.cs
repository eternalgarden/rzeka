using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Rzeka.Tests.Integration;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Rx
{
    public class Rzeka_Rx_Understanding_Hot_To_Cold_Observable_Chains : TestBase
    {
        [UnityTest]
        public IEnumerator a_Do_NOT_getting_called_on_cold_observable()
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
        public IEnumerator b_Do_GETS_called_on_hot_observable()
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

            using var lateObserver = anotherObservable.Subscribe();

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
                .Replay(1); // * This time with replay instead

            IObservable<int> anotherObservable = connectableObservable
                .Do(onNext: _ => count++);

            using var connection = connectableObservable.Connect();

            using var lateObserver = anotherObservable.Subscribe();

            yield return null;

            AssertEqual(1, count);

            // -------------
        }

        [UnityTest]
        public IEnumerator f_HotToCold_stacked_observable_with_TWO_observers()
        {
            // -------------

            int count = 0;

            IConnectableObservable<int> connectableObservable = Observable
                .Return(1)
                .Replay(1); // * This time with replay instead

            IObservable<int> anotherObservable = connectableObservable
                .Do(onNext: _ => count++);

            using var connection = connectableObservable.Connect();

            using var lateObserver1 = anotherObservable.Subscribe();
            using var lateObserver2 = anotherObservable.Subscribe();

            yield return null;

            AssertEqual(2, count);

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
                .Replay(1); // * This time with replay instead

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