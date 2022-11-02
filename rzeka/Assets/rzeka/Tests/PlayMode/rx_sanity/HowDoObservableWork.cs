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
    public class Rzeka_Rx_SanityChecks : TestBase
    {
        [UnityTest]
        public IEnumerator a_WhenDoGetsCalled1()
        {
            // -------------

            int count = 0;

            var observable1 = Observable
                .Return(1)
                .Do(onNext: _ => count++);

            var observable2 = observable1
                .Select(x => $"number {x}")
                .Do(onNext: _ => count++);

            yield return null;

            AssertEqual(0, count);

            // -------------
        }

        [UnityTest]
        public IEnumerator a_WhenDoGetsCalled2()
        {
            // -------------

            int count = 0;

            IObservable<int> observable1 = Observable
                .Return(1)
                .Do(onNext: _ => count++);

            Func<IObservable<int>, IObservable<string>> theoryLoom = observable => observable
                .Select(x => $"number {x}")
                .Do(onNext: _ => count++);

            theoryLoom.Invoke(observable1);

            yield return null;

            AssertEqual(0, count);

            // -------------
        }

        [UnityTest]
        public IEnumerator a_WhenDoGetsCalled3()
        {
            // -------------

            int count = 0;

            IObservable<int> observable1 = Observable
                .Return(1);

            // * only moved Do to a separeate observable step to mimic how rzeka does it inside scrolls
            IObservable<int> observable2 = observable1
                .Do(onNext: _ => count++);

            Func<IObservable<int>, IObservable<string>> theoryLoom = observable => observable
                .Select(x => $"number {x}")
                .Do(onNext: _ => count++);

            theoryLoom.Invoke(observable2);

            yield return null;

            AssertEqual(0, count);

            // -------------
        }


        [UnityTest]
        public IEnumerator a_WhenDoGetsCalled4()
        {
            // -------------

            int count = 0;

            IObservable<int> observable1 = Observable
                .Return(1);

            // * only moved Do to a separeate observable step to mimic how rzeka does it inside scrolls
            IObservable<int> observable2 = observable1
                .Do(onNext: _ => count++);

            Func<IObservable<int>, IObservable<string>> theoryLoom = observable => observable
                .Select(x => $"number {x}");

            IObservable<string> observable3 = theoryLoom.Invoke(observable2);

            IObservable<string> observable4 = observable3
                .Do(onNext: _ => count++);

            yield return null;

            AssertEqual(0, count);

            // -------------
        }

        [UnityTest]
        public IEnumerator b_WhenDoGetsCalled4()
        {
            // -------------

            int countCnonjuring = 0;
            int countLoom = 0;

            IObservable<int> conjuring = Observable
                .Return(1)
                .Do(onNext: _ => countCnonjuring++);

            // * only moved Do to a separeate observable step to mimic how rzeka does it inside scrolls

            Func<IObservable<int>, IObservable<string>> loomSpell = observable => observable
                .Select(x => $"number {x}");

            IObservable<string> loom = loomSpell.Invoke(conjuring)
                .Do(onNext: _ => countLoom++);

            var weaving = Observer.Create<string>(
                onNext: next => { });

            using var sub = loom.Subscribe(weaving);

            yield return null;

            AssertEqual((1,1), (countCnonjuring,countLoom));

            // -------------
        }
        
        [UnityTest]
        public IEnumerator c1_Ugh()
        {
            // -------------

            int countCnonjuring = 0;
            int countLoom = 0;

            IConnectableObservable<int> connectible = Observable
                    .Timer(TimeSpan.FromSeconds(0.1))
                    .Select(x => (int)x)
                    .Publish();

            using var x = connectible.Connect();
            
            var conjuring = connectible
                .Do(onNext: _ => countCnonjuring++);

            // * only moved Do to a separeate observable step to mimic how rzeka does it inside scrolls

            Func<IObservable<int>, IObservable<string>> loomSpell = observable => observable
                .Select(x => $"number {x}");

            IObservable<string> loom = loomSpell.Invoke(conjuring)
                .Do(onNext: _ => countLoom++);

            var weaving = Observer.Create<string>(
                onNext: next => { });

            using var sub = loom.Subscribe(weaving);

            yield return new WaitForSeconds(0.2f);

            AssertEqual((1,1), (countCnonjuring,countLoom));

            // -------------
        }
        
        [UnityTest]
        public IEnumerator c2_Ugh()
        {
            // -------------

            int countCnonjuring = 0;
            int countLoom = 0;

            IConnectableObservable<int> connectible = Observable
                .Timer(TimeSpan.FromSeconds(0.1))
                .Select(x => (int)x)
                .Publish();

            using var x = connectible.Connect();
            
            var conjuring = connectible
                .Do(onNext: _ => countCnonjuring++);

            // * only moved Do to a separeate observable step to mimic how rzeka does it inside scrolls

            Func<IObservable<int>, IObservable<string>> loomSpell = observable => observable
                .Select(x => $"number {x}");

            IObservable<string> loom = loomSpell.Invoke(conjuring)
                .Do(onNext: _ => countLoom++);

            var weaving = Observer.Create<string>(
                onNext: next => { });

            using var sub1 = loom.Subscribe(weaving);
            using var sub2 = loom.Subscribe(weaving);

            yield return new WaitForSeconds(0.2f);

            AssertEqual((2,2), (countCnonjuring,countLoom));

            // -------------
        }
        
        [UnityTest]
        public IEnumerator c2a_Ugh()
        {
            // -------------

            int countCnonjuring = 0;
            int countLoom = 0;

            IConnectableObservable<int> connectible = Observable
                .Timer(TimeSpan.FromSeconds(0.1))
                .Select(x => (int)x)
                .Publish();

            using var x = connectible.Connect();
            
            var conjuring = connectible
                .Do(onNext: _ => countCnonjuring++);

            // * only moved Do to a separeate observable step to mimic how rzeka does it inside scrolls

            Func<IObservable<int>, IObservable<string>> loomSpell = observable => observable
                .Select(x => $"number {x}");

            IObservable<string> loom = loomSpell.Invoke(conjuring)
                .Do(onNext: _ => countLoom++);

            var weaving = Observer.Create<string>(
                onNext: next => { });

            using var sub1 = loom.Subscribe(weaving);
            using var sub2 = loom.Subscribe(weaving);

            yield return new WaitForSeconds(0.2f);
            
            using var sub3 = loom.Subscribe(weaving);

            AssertEqual((2,2), (countCnonjuring,countLoom));

            // -------------
        }
    }
}