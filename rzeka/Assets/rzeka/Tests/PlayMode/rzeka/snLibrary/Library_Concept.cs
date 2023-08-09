using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Library
{
    public class Library_Concept : MonoBehaviour
    {
        ITestableRzeka _rzeka;
        Rzeka.Library _library;
        TestTools _tools;

        CollectibleDisposable Q { get; set; }

        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            _rzeka = new SpringRiver();
            _library = _rzeka.Library;
            _tools = new TestTools(_rzeka);
            Q = new();

            yield return null;

            // -------------
        }

        [UnityTearDown]
        public virtual IEnumerator Teardown()
        {
            // -------------
            
            _rzeka.Dispose();
            Q.Dispose();

            yield return null;

            // -------------
        }
        
        [Test]
        public void a_Unfortunately_you_cant_use_a_single_subject_as_proxy()
        {
            int count = 0;
            
            Subject<int> stream = new();

            Q += stream.Subscribe(x =>
            {
                Debug.Log($"<color=orange>{x.ToString()}</color>");
                count++;
            });

            var o1 = Observable.Return(1);
            var o2 = Observable.Return(2);

            Q += o1.Subscribe(stream);
            Q += o2.Subscribe(stream);  
            
            // * However I would initially hope it would be '2' 
            // * So that a single subject could serve as a proxy between its observables and observers
            TestTools.AssertEqual(1, count);
        }
        
        [Test]
        public void b_However_an_additional_implied_subject_works()
        {
            int count = 0;
            
            Subject<int> stream = new();

            Q += stream.Subscribe(x =>
            {
                count++;
            });

            var o1 = Observable.Return(1);
            var o2 = Observable.Return(2);
            
            /*
             * this .Subscribe overload creates an observer under the hood
             * this observer is our implied subject
             */
            Q += o1.Subscribe(x => stream.OnNext(x));
            Q += o2.Subscribe(x => stream.OnNext(x));  
            
            TestTools.AssertEqual(2, count);
        }
        
        [Test]
        public void c_Using_an_Observer_as_the_proxy()
        {
            int count = 0;
            
            Subject<int> stream = new();

            Q += stream.Subscribe(x =>
            {
                Debug.Log($"<color=orange>{x.ToString()}</color>");
                count++;
            });

            var o1 = Observable.Return(1);
            var o2 = Observable.Return(2);
            
            IObserver<int> proxy = Observer.Create<int>(x => stream.OnNext(x));

            /*
             * here an anonymous observer serves as a proxy
             */
            Q += o1.Subscribe(proxy);
            Q += o2.Subscribe(proxy);  
            
            TestTools.AssertEqual(2, count);
        }
    }
}
