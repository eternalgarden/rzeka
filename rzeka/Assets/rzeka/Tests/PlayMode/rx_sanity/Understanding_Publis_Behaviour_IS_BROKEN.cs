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
    public class Understanding_Publish_Behaviour_IS_BROKEN
    {
        // -------------
        
        [UnityTest]
        public IEnumerator a_behaviour_publish_is_BROKEN_it_doesnt_provide()
        {
            var Q = new CollectibleDisposable();

            var interval = Observable
                .Return<int>(value: 1)
                .Publish(int.MinValue); // * This is a Publish overload that uses BehaviourSubject in background

            Q += interval.Connect(); // Thus the default value should be skipped but the .Return value of 1 should be received

            int receivals = 0;

            Q += interval
                .Subscribe(_ => receivals++);

            yield return null;

            TestTools.AssertEqual(0, receivals); // ! THERE ARE 0 RECEIVALS AND SHOULD BE 1

            Q.Dispose();
        }

        [UnityTest]
        public IEnumerator b_behaviour_multicast_is_BROKEN_aswell()
        {
            var Q = new CollectibleDisposable();

            const int FINAL = 1;

            var interval = Observable
                .Return<int>(FINAL)
                .Multicast(new BehaviorSubject<int>(value: int.MinValue)); // * Here behaviour subject is used explicitly

            Q += interval.Connect();

            int receivals = 0;

            Q += interval
                .Subscribe(_ => receivals++);

            yield return null;

            TestTools.AssertEqual(0, receivals); // ! THERE ARE 0 RECEIVALS AND SHOULD BE 1

            Q.Dispose();
        }

        [UnityTest]
        public IEnumerator c_use_replay_multicast_with_buffer_size_of_one_instead()
        {
            var Q = new CollectibleDisposable();

            var interval = Observable
                .Return<int>(6)
                .Multicast(new ReplaySubject<int>(bufferSize: 1));

            Q += interval.Connect();

            int receivals = 0;

            Q += interval
                .Subscribe(_ => receivals++);

            yield return null;

            TestTools.AssertEqual(1, receivals); // * CORRECTLY REGISTERS ONE RECEIVED VALUE

            Q.Dispose();
        }

        [UnityTest]
        public IEnumerator d_use_replay_multicast_with_buffer_size_of_one_instead_verified()
        {
            var Q = new CollectibleDisposable();

            const int FINAL = 6;

            var interval = Observable
                .Return<int>(FINAL)
                .Multicast(new ReplaySubject<int>(bufferSize: 1));

            Q += interval.Connect();

            bool receivedFinal = false;

            Q += interval
                .Subscribe(i => receivedFinal = i == FINAL);

            yield return null;

            TestTools.AssertEqual(true, receivedFinal); // * CORRECTLY RECEIVES THE EXPECTED FINAL VALUE

            Q.Dispose();
        }

        [UnityTest]
        public IEnumerator e_use_replay_multicast_with_buffer_size_of_one_instead_verified_refcount()
        {
            var Q = new CollectibleDisposable();

            const int FINAL = 6;

            var interval = Observable
                .Return<int>(FINAL)
                .Multicast(new ReplaySubject<int>(bufferSize: 1))
                .RefCount();

            int receivals = 0;

            Q += interval
                .Subscribe(_ => receivals++);

            yield return null;

            TestTools.AssertEqual(1, receivals); // * CORRECTLY REGISTERS ONE RECEIVED VALUE

            Q.Dispose();
        }

        [UnityTest]
        public IEnumerator f_use_replay_multicast_with_buffer_size_of_one_instead_verified_autoconnect()
        {
            var Q = new CollectibleDisposable();

            const int FINAL = 6;

            var interval = Observable
                .Return<int>(FINAL)
                .Multicast(new ReplaySubject<int>(bufferSize: 1))
                .AutoConnect();

            int receivals = 0;

            Q += interval
                .Subscribe(_ => receivals++);

            yield return null;

            TestTools.AssertEqual(1, receivals); // * CORRECTLY REGISTERS ONE RECEIVED VALUE

            Q.Dispose();
        }
    
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 30 November 2022 🌊 */