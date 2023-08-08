/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using NUnit.Framework;
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Occurences.Matter
{
    public class Loom_Multiple_Input_Overloads
    {
        // -------------

        ITestableRzeka Rzeka;
        TestTools _tools;
        CollectibleDisposable Q { get; set; }

        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            Rzeka = new SpringRiver();
            _tools = new TestTools(Rzeka);
            Q = new();

            yield return null;

            // -------------
        }

        [UnityTearDown]
        public virtual IEnumerator Teardown()
        {
            // -------------

            Rzeka.Dispose();
            Q.Dispose();

            yield return null;

            // -------------
        }
        
        [Test]
        [TestCase(MatterOccurenceCategory.Shaped, typeof(ANumber))]
        [TestCase(MatterOccurenceCategory.Received, typeof(ANumber))]
        [TestCase(MatterOccurenceCategory.Shaped, typeof(AName))]
        [TestCase(MatterOccurenceCategory.Received, typeof(AName))]
        [TestCase(MatterOccurenceCategory.Shaped, typeof(UserData))]
        [TestCase(MatterOccurenceCategory.Received, typeof(UserData))]
        public void a1_loom_2_shaped_receiveable_matter(MatterOccurenceCategory category, Type type)
        {
            // -------------
            
            bool registered = false;
            
            using var m1 = Rzeka.Eris.MatterOccurences
                .Where(m => m.MatterOccurenceCategory == category)
                .Where(m => m.Matter.GetType() == type)
                .Subscribe(_ => registered = true);

            using var loom = Rzeka.Loom<ANumber, AName, UserData>(
                who: this,
                spell: source => source
                    .Select(glyph =>
                    {
                        return new UserData(glyph.Two.Name, "Cancer", glyph.One.Number);
                    }));

            using var d1 = _tools.Weave_UserData();

            using var s1 = _tools.Strand_ANumber_Synchronous(1);
            using var s2 = _tools.Strand_AName_Synchronous("fluff");

            TestTools.AssertEqual(true, registered);

            // -------------
        }

        [UnityTest]
        public IEnumerator b()
        {
            int count = 0;
            
            // TODO would it make sense for a variant like this
            // Q += _rzeka.Eris.MatterOccurences
            //     .Where(m => m.MatterOccurenceCategory == category)
            //     .Where(m => m.Matter.GetType() == type)
            //     .Subscribe(_ => count++);

            Q += Rzeka.Weave<ArbitraryMatter3>(
                this,
                next => next.Subscribe(m =>
                {
                    count++;
                }));
            
            IDisposable strand1a = Rzeka.Strand<ArbitraryMatter1>(
                this,
                Observable.Return(new ArbitraryMatter1("1")));

            Q += Rzeka.Loom<ArbitraryMatter1, ArbitraryMatter2, ArbitraryMatter3>(
                this,
                x => x.Select(x =>
                {
                    Debug.Log($"<color=red>{x.Two.Text}</color>");
                    return new ArbitraryMatter3("3");
                }));

            
            IDisposable strand2 = Rzeka.Strand<ArbitraryMatter2>(
                this,
                Observable
                        .Timer(TimeSpan.FromSeconds(0.5))
                        .Select(_ => new ArbitraryMatter2("2")));
            
            strand1a.Dispose();
            
            yield return new WaitForSeconds(0.7f);
            
            IDisposable strand1b = Rzeka.Strand<ArbitraryMatter1>(
                this,
                Observable.Return(new ArbitraryMatter1("1")));
            
            strand1b.Dispose();
            strand2.Dispose();
            
            TestTools.AssertEqual(1, count);
        }

        [UnityTest]
        public IEnumerator b1_Late_Loom_will_not_produce_an_output()
        {
            int count = 0;

            Q += Rzeka.Weave<ArbitraryMatter3>(
                this,
                next => next.Subscribe(m =>
                {
                    count++;
                }));
            
            Q += Rzeka.Strand<ArbitraryMatter1>(
                this,
                Observable.Return(new ArbitraryMatter1("1")));
            
            Q += Rzeka.Strand<ArbitraryMatter2>(
                this,
                Observable.Return(new ArbitraryMatter2("2")));

            Q += Rzeka.Loom<ArbitraryMatter1, ArbitraryMatter2, ArbitraryMatter3>(
                this,
                x => x.Select(x =>
                {
                    Debug.Log($"<color=red>{x.Two.Text}</color>");
                    return new ArbitraryMatter3("3");
                }));
            
            yield return new WaitForSeconds(0.1f);
            
            TestTools.AssertEqual(0, count);
        }
        
        [UnityTest]
        public IEnumerator b2_Neither_will_partially_late()
        {
            int count = 0;

            Q += Rzeka.Weave<ArbitraryMatter3>(
                this,
                next => next.Subscribe(m =>
                {
                    count++;
                }));
            
            Q += Rzeka.Strand<ArbitraryMatter1>(
                this,
                Observable.Return(new ArbitraryMatter1("1")));

            Q += Rzeka.Loom<ArbitraryMatter1, ArbitraryMatter2, ArbitraryMatter3>(
                this,
                x => x.Select(x =>
                {
                    Debug.Log($"<color=red>{x.Two.Text}</color>");
                    return new ArbitraryMatter3("3");
                }));
            
            Q += Rzeka.Strand<ArbitraryMatter2>(
                this,
                Observable.Return(new ArbitraryMatter2("2")));
            
            yield return new WaitForSeconds(0.1f);
            
            TestTools.AssertEqual(0, count);
        }
        
        [UnityTest]
        public IEnumerator e()
        {
            int count = 0;

            Q += Rzeka.Weave<ArbitraryMatter3>(
                this,
                next => next.Subscribe(m =>
                {
                    count++;
                }));
            
            Q += Rzeka.Loom<ArbitraryMatter1, ArbitraryMatter2, ArbitraryMatter3>(
                this,
                x => x.Select(x =>
                {
                    Debug.Log($"<color=red>{x.Two.Text}</color>");
                    return new ArbitraryMatter3("3");
                }));
            
            Q += Rzeka.Strand<ArbitraryMatter1>(
                this,
                Observable.Return(new ArbitraryMatter1("1")));
            
            IDisposable s = Rzeka.Strand<ArbitraryMatter2>(
                this,
                Observable.Return(new ArbitraryMatter2("2")));
            
            s.Dispose();
            
            Q += Rzeka.Strand<ArbitraryMatter2>(
                this,
                Observable.Return(new ArbitraryMatter2("2")));
            
            yield return new WaitForSeconds(0.7f);
            
            TestTools.AssertEqual(1, count);
        }
    }
}