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

namespace Rzeka.Tests.Overloads.Weave
{
    public class Overloads_Weave_3_Glyph
    {
        ITestableRzeka _rzeka;
        Rzeka.Library _library;
        TestTools _tools;

        CollectibleDisposable Dependencies { get; set; }
    
        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            
            _rzeka = new SpringRiver();
            _library = _rzeka.Library;
            _tools = new TestTools(_rzeka);

            Dependencies = new();
             Dependencies += _rzeka.Strand<ArbitraryMatter1>(
                 this,
                 Observable.Return(new ArbitraryMatter1("some")));
            
             Dependencies += _rzeka.Strand<ArbitraryMatter2>(
                 this,
                 Observable.Return(new ArbitraryMatter2("flower")));
            
             Dependencies += _rzeka.Strand<ArbitraryMatter3>(
                 this,
                 Observable.Return(new ArbitraryMatter3("blooming")));

            yield return null;

            // -------------
        }

        [UnityTearDown]
        public virtual IEnumerator Teardown()
        {
            // -------------
            
            _rzeka.Dispose();
            Dependencies.Dispose();
            
            yield return null;

            // -------------
        }
        
        [Test]
        [TestCase(SpellOccurenceCategory.Created, 1)]
        [TestCase(SpellOccurenceCategory.HasMana, 1)]
        [TestCase(SpellOccurenceCategory.NoMana, 0)]
        public void a_correct_creation_spell_occurences(SpellOccurenceCategory category, int expected)
        {
            // -------------
            
            int count = 0;
            
            using var m1 = _rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source is WeavingSpell3Glyph<ArbitraryMatter1, ArbitraryMatter2, ArbitraryMatter3>)
                .Subscribe(_ => count++);
            
            using var weave = _rzeka.Weave<
                ArbitraryMatter1, 
                ArbitraryMatter2, 
                ArbitraryMatter3>(
                this,
                source => source.Subscribe());
            
            Assert.AreEqual(expected, count);
        
            // -------------
        }
        
        [Test]
        [TestCase(SpellOccurenceCategory.Created, 1)]
        [TestCase(SpellOccurenceCategory.HasMana, 0)]
        [TestCase(SpellOccurenceCategory.NoMana, 1)]
        public void b_correct_creation_spell_occurences_nomana(SpellOccurenceCategory category, int expected)
        {
            // -------------
            
            Dependencies.Dispose();
            
            int count = 0;
            
            using var m1 = _rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source is WeavingSpell3Glyph<ArbitraryMatter1, ArbitraryMatter2, ArbitraryMatter3>)
                .Subscribe(_ => count++);
            
            using var weave = _rzeka.Weave<
                ArbitraryMatter1, 
                ArbitraryMatter2, 
                ArbitraryMatter3>(
                this,
                source => source.Subscribe());
            
            Assert.AreEqual(expected, count);
        
            // -------------
        }
        
        // [Test]
        // public void c_receives_one_matter_late_subscriber()
        // {
        //     // -------------
        //
        //     int count = 0;
        //     
        //     using var weave = _rzeka.Weave<
        //         ArbitraryMatter1, 
        //         ArbitraryMatter2, 
        //         ArbitraryMatter3>(
        //         this,
        //         source => source
        //             .Subscribe(glyph =>
        //             {
        //                 count++;
        //             }));
        //     
        //     Assert.AreEqual(1, count);
        //
        //     // -------------
        // }
    }
}
