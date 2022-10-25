using System;
using System.Collections;
using System.Reactive.Linq;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Rx
{
    public class HowDoObservableWork : TestBase
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

            Func<IObservable<int>,IObservable<string>> theoryLoom = observable => observable
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

            Func<IObservable<int>,IObservable<string>> theoryLoom = observable => observable
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
    }
}
