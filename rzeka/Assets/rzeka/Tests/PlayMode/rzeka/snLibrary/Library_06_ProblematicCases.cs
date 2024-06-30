
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Library
{
    public class Library_06_ProblematicCases
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
        public IEnumerator a_()
        {
            IDisposable stranding = null;

            int[] receivals = new int[3];
            
            Q += _rzeka.Weave<ArbitraryMatter1>(
                this,
                Observer.Create<ArbitraryMatter1>(_ =>
                {
                    Debug.Log($"<color=cyan>oik1</color>");
                    receivals[0] = 1;
                } ));

            Q += _rzeka.Weave<ArbitraryMatter1>(
                this,
                Observer.Create<ArbitraryMatter1>(_ =>
                {
                    Debug.Log($"<color=yellow>oik</color>");
                    receivals[1] = 1;
                    stranding.Dispose();
                } ));

            Q += _rzeka.Weave<ArbitraryMatter1>(
                this,
                Observer.Create<ArbitraryMatter1>(_ =>
                {
                    Debug.Log($"<color=cyan>oik3</color>");
                    receivals[2] = 1;
                } ));

            stranding = _rzeka.Strand<ArbitraryMatter1>(
                this,
                Observable
                    .Timer(TimeSpan.FromSeconds(0.5), Scheduler.Immediate)
                    .Select(_ => new ArbitraryMatter1("oik")));
            
            yield return new WaitForSeconds(0.7f);
            
            TestTools.AssertEqual(new int[] { 1,1,1 }, receivals, (x,y) => x.SequenceEqual(y));
        }
        
        [Test]
        public void b_()
        {
            IDisposable stranding = null;

            int[] receivals = new int[3]; //
            
            Q += _rzeka.Weave<ArbitraryMatter2>(
                this,
                Observer.Create<ArbitraryMatter2>(_ =>
                {
                    Debug.Log($"<color=cyan>oik1</color>");
                    receivals[0] = 1;
                } ));

            Q += _rzeka.Weave<ArbitraryMatter2>(
                this,
                Observer.Create<ArbitraryMatter2>(_ =>
                {
                    Debug.Log($"<color=yellow>oik</color>");
                    receivals[1] = 1;
                    stranding.Dispose();
                } ));
            
            Q += _rzeka.Weave<ArbitraryMatter2>(
                this,
                Observer.Create<ArbitraryMatter2>(_ =>
                {
                    Debug.Log($"<color=cyan>oik3</color>");
                    receivals[2] = 1;
                } ));

            stranding = _rzeka.Loom<ArbitraryMatter1,ArbitraryMatter2>(
                this,
                arm1 => arm1
                    .Select(_ => new ArbitraryMatter2()));

            Q += _rzeka.Strand<ArbitraryMatter1>(
                this,
                Observable
                    // .Timer(TimeSpan.FromSeconds(0.5))
                    .Return(new ArbitraryMatter1("oik")));
            
            TestTools.AssertEqual(new int[] { 1,1,1 }, receivals, (x,y) => x.SequenceEqual(y));
        }
    }
}
