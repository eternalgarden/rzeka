
using NUnit.Framework;
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public class A_Little_Example
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
        
        [UnityTest]
        public IEnumerator a_Is_Weave_UserWelcomingText_Received()
        {
            // -------------

            int count = 0;
            Q += _rzeka.Weave<ArbitraryMatter2>(
                this,
                Observer.Create<ArbitraryMatter2>(next =>
                {
                    Debug.Log($"<color=yellow>{next.Text}</color>");
                    count++;
                }));

            Q += _rzeka.Loom<ArbitraryMatter1, ArbitraryMatter2>(
                this,
                input => input
                    .Select(_ =>
                    {
                        return new ArbitraryMatter2("pops");
                    }));
            
            Q += _rzeka.Strand<ArbitraryMatter1>(
                this,
                Observable.Return(new ArbitraryMatter1("some")));
            
            Q += _rzeka.Strand<ArbitraryMatter2>(
                this,
                Observable.Return(new ArbitraryMatter2("flower")));

            yield return null;

            Assert.AreEqual(2, count);

            // -------------
        }
    }
}