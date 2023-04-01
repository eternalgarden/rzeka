using System.Reactive.Linq;
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Subjects;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Rx
{
    public class Understanding_Merge
    {
        CollectibleDisposable Q { get; set; }

        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            Q = new();

            yield return null;

            // -------------
        }

        [UnityTearDown]
        public virtual IEnumerator Teardown()
        {
            // -------------
            
            Q.Dispose();

            yield return null;

            // -------------
        }
        
        [Test]
        public void a_subject_as_observer_of_mege_stream()
        {
            var o1 = Observable.Return("oik!");
            var o2 = Observable.Return("nifty!");

            var merge = o1.Merge(o2);

            int count = 0;
            using var sub = merge.Subscribe(_ => count++);
            
            Assert.AreEqual(2, count);
        }
        
        [Test]
        public void c_oik()
        {
            var subject = new Subject<string>();
            int count = 0;
            using var sub = subject.Subscribe(next =>
            {
                Debug.Log($"<color=yellow>{next}</color>");
                count++;
            });
            
            var o1 = Observable.Return("oik!");

            var d1 = o1.Subscribe(subject);
            
            d1.Dispose();
            
            var o2 = Observable.Return("nifty!");
            var merge = o1.Merge(o2);

            using var d = merge.Subscribe(subject);
            
            Assert.AreEqual(2, count);
        }
        
        [Test]
        public void d_oik()
        {
            var subject = new Subject<string>();
            var obs = Observer.Create<string>(next => subject.OnNext(next));
                
            int count = 0;
            using var sub = subject.Subscribe(next =>
            {
                // Debug.Log($"<color=yellow>{next}</color>");
                count++;
            });
            
            var o1 = Observable.Return("oik!");

            var d1 = o1.Subscribe(obs);
            
            d1.Dispose();
            
            var o2 = Observable.Return("nifty!");
            var merge = o1.Merge(o2);

            using var d = merge.Subscribe(obs);
            
            Assert.AreEqual(2, count);
        }
        
        [Test]
        public void e_oik()
        {
            var subject = new Subject<string>();
            var obs = Observer.Create<string>(next => subject.OnNext(next));
                
            int count = 0;
            using var sub = subject.Subscribe(next =>
            {
                // Debug.Log($"<color=yellow>{next}</color>");
                count++;
            });
            
            var o1 = Observable.Return("oik!");
            
            var subo1 = new Subject<string>();
            var d1 = subo1.Subscribe(obs);
            
            using var do1 = o1.Subscribe(subo1);
            
            d1.Dispose();
            
            var o2 = Observable.Return("nifty!");
            var subo2 = new Subject<string>();

            var merge = subo1.Merge(subo2);

            using var d = merge.Subscribe(obs);
            
            using var do2 = o2.Subscribe(subo2);
            
            Assert.AreEqual(2, count);
        }
        
        [UnityTest]
        public IEnumerator f_oik()
        {
            var subject = new Subject<string>();
            var observer = Observer.Create<string>(next => subject.OnNext(next));
                
            int count = 0;
            using var sub = subject.Subscribe(next =>
            {
                count++;
                // Debug.Log($"<color=yellow>{next}</color>");
            });

            var o1 = Observable
                .Interval(TimeSpan.FromSeconds(0.5))
                .Select(x => x.ToString());
            var subo1 = new Subject<string>();
            
            var d1 = subo1.Subscribe(observer);
            
            using var do1 = o1.Subscribe(subo1);

            yield return new WaitForSeconds(0.7f);
            
            d1.Dispose();
            
            var o2 = Observable.Return("nifty!");
            var subo2 = new Subject<string>();

            var merge = subo1.Merge(subo2);
            using var d = merge.Subscribe(observer);
            
            using var do2 = o2.Subscribe(subo2);
            
            yield return new WaitForSeconds(0.7f);
            
            Assert.AreEqual(3, count);
            
            // Debug.Log($"<color=green>hilfe</color>");
        }
        
        [UnityTest]
        public IEnumerator g_oik()
        {
            var subject = new Subject<string>();
            var observer = Observer.Create<string>(next => subject.OnNext(next));
                
            int count = 0;
            Q += subject.Subscribe(next =>
            {
                count++;
                // Debug.Log($"<color=yellow>{next}</color>");
            });
            
            
            var subo1 = new Subject<string>();
            Q += subo1.Subscribe(observer);

            var o1 = Observable
                .Interval(TimeSpan.FromSeconds(0.5))
                .Select(_ => "oik!");
            var d1 = o1.Subscribe(next => subo1.OnNext(next));

            yield return new WaitForSeconds(0.7f);
            
            d1.Dispose();
            
            // yield return new WaitForSeconds(1f);
            
            Debug.Log($"<color=green>{count}</color>");
            
            Assert.AreEqual(1, count);
            
        }
    }
}