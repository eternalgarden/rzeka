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
    public class Understanding_Subjects
    {
        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(5)]
        public void a_subject_pushes_to_its_subscribers(int pushedValuesCount)
        {
            var q = new CollectibleDisposable();

            Subject<int> subject = new Subject<int>();
            
            int receivals = 0; // * EXPECTED 2

            q += subject.Subscribe(i => receivals++);
            q += subject.Subscribe(i => receivals++);

            for (int i = 0; i < pushedValuesCount; i++)
            {
                subject.OnNext(1);
            }

            q.Dispose();
            
            Assert.AreEqual(pushedValuesCount*2, receivals);
        }

        [Test]
        [TestCase(0)]
        [TestCase(1)]
        [TestCase(7)]
        public void b_subject_as_both_observer_and_observable(int count)
        {
            IObservable<int> source = Observable.Range(0,count);

            using Subject<int> subject = new Subject<int>();

            int receivals = 0;

            using var o1 = subject.Subscribe(i => receivals++);
            using var o2 = subject.Subscribe(i => receivals++);

            using var oSub = source.Subscribe(subject);

            Assert.AreEqual(count*2, receivals);
        }
        
        [UnityTest]
        public IEnumerator c_Subject_is_HOT()
        {
            IObservable<int> source = Observable.Return<int>(1);

            using Subject<int> subject = new Subject<int>();
            
            // * immediate subscriptions
            using var oSub = source.Subscribe(subject);
            
            int receivals = 0;

            using var o1 = subject.Subscribe(i => receivals++);
            using var o2 = subject.Subscribe(i => receivals++);

            yield return null;
            
            // * notice in this case we will have 0 receivals since subject is a hot observable
            Assert.AreEqual(0, receivals);
        }
        
        [UnityTest]
        public IEnumerator d_ReplaySubject()
        {
            IObservable<int> source = Observable.Return<int>(1);

            // * using replay subject with buffer one insetead
            using ReplaySubject<int> subject = new ReplaySubject<int>(1);
            
            // * immediate subscriptions
            using var oSub = source.Subscribe(subject);
            
            int receivals = 0;

            using var o1 = subject.Subscribe(i => receivals++);
            using var o2 = subject.Subscribe(i => receivals++);

            yield return null;
            
            // 2 receivals again
            Assert.AreEqual(2, receivals);
        }
        
        [UnityTest]
        public IEnumerator e_ReplaySubject()
        {
            using Subject<int> source1 = new Subject<int>();
            using Subject<int> subject = new Subject<int>();
            
            IDisposable source1sub = source1.AsObservable().Subscribe(subject);

            int receivals = 0;

            using var o1 = subject.Subscribe(i => receivals++);
            
            source1.OnNext(1);
            source1sub.Dispose();
            
            using Subject<int> source2 = new Subject<int>();
            
            IDisposable source2sub = source2.AsObservable().Subscribe(subject);
            
            source2.OnNext(1);
            source2sub.Dispose();

            yield return null;
            
            // 2 receivals again
            Assert.AreEqual(2, receivals);
        }
    }
}