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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Library
{
    public class Library_04_MultipleProviders
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

        [Test] public void a_double_conjurer_counts_as_one_stream()
        {
            // -------------

            using var s1 = _tools.Strand_AName_Synchronous("sally");
            using var s2 = _tools.Strand_AName_Synchronous("barbara");

            TestTools.AssertEqual(1, _library.StreamsCount);

            // -------------
        }
        
        [Test]
        public void b_strands_of_same_type()
        {
            // -------------

            int count = 0;
            Q += _rzeka.Weave<ArbitraryMatter1>(
                this,
                Observer.Create<ArbitraryMatter1>(next =>
                {
                    // Debug.Log($"<color=yellow>{next.Text}</color>");
                    count++;
                }));
            
            Q += _rzeka.Strand<ArbitraryMatter1>(
                this,
                Observable.Return(new ArbitraryMatter1("birds")));
            
            Q += _rzeka.Strand<ArbitraryMatter1>(
                this,
                Observable.Return(new ArbitraryMatter1("chirping")));

            Assert.AreEqual(2, count);

            // -------------
        }
        
        
        [Test]
        public void c_strand_and_a_loom_of_same_type()
        {
            // -------------

            int count = 0;
            Q += _rzeka.Weave<ArbitraryMatter2>( // 1
                this,
                Observer.Create<ArbitraryMatter2>(_ => count++));
            
            Q += _rzeka.Strand<ArbitraryMatter2>(
                this,
                Observable.Return(new ArbitraryMatter2("flower")));

            Q += _rzeka.Loom<ArbitraryMatter1, ArbitraryMatter2>(
                this,
                input => input
                    .Select(_ =>
                    {
                        return new ArbitraryMatter2("pops");
                    }));
            
            Q += _rzeka.Strand<ArbitraryMatter1>( // 2
                this,
                Observable.Return(new ArbitraryMatter1("some")));
            
            Q += _rzeka.Strand<ArbitraryMatter1>( // 3
                this,
                Observable.Return(new ArbitraryMatter1("kitty")));

            Assert.AreEqual(3, count);

            // -------------
        }
    }
}
