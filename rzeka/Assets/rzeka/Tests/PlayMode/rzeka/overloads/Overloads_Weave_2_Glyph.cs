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
    public class Overloads_Weave_2_Glyph
    {
        ITestableRzeka Rzeka;
        Rzeka.Library _library;
        TestTools _tools;

        CollectibleDisposable Q { get; set; }
    
        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            
            Rzeka = new SpringRiver();
            _library = Rzeka.Library;
            _tools = new TestTools(Rzeka);

            Q = new();
             // Q += _rzeka.Strand<ArbitraryMatter1>(
             //     this,
             //     Observable.Return(new ArbitraryMatter1("some")));
             //
             // Q += _rzeka.Strand<ArbitraryMatter2>(
             //     this,
             //     Observable.Return(new ArbitraryMatter2("flower")));

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
        [TestCase(SpellOccurenceCategory.Created, 1)]
        [TestCase(SpellOccurenceCategory.HasMana, 0)]
        [TestCase(SpellOccurenceCategory.NoMana, 1)]
        public void a_correct_creation_spell_occurences(SpellOccurenceCategory category, int expected)
        {
            // -------------
            
            int count = 0;
            
            using var m1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                // .Where(occ => occ.Source is WeavingSpell2Glyph<ArbitraryMatter1, ArbitraryMatter2>)
                .Subscribe(_ => count++);
            
            using var weave = Rzeka.Weave<
                ArbitraryMatter1, 
                ArbitraryMatter2>(
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
            
            Q.Dispose();
            
            int count = 0;
            
            using var m1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source is WeavingSpell2Glyph<ArbitraryMatter1, ArbitraryMatter2>)
                .Subscribe(_ => count++);
            
            using var weave = Rzeka.Weave<
                ArbitraryMatter1, 
                ArbitraryMatter2>(
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
        //         ArbitraryMatter2>(
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
