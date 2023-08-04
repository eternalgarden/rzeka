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
    public class Library_05_Overloads
    {
        ITestableRzeka _rzeka;
        Rzeka.Library _library;
        TestTools _tools;
    
        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            _rzeka = new SpringRiver();
            _library = _rzeka.Library;
            _tools = new TestTools(_rzeka);

            yield return null;

            // -------------
        }

        [UnityTearDown]
        public virtual IEnumerator Teardown()
        {
            // -------------
            
            _rzeka.Dispose();

            yield return null;

            // -------------
        }
        
        // [Test]
        // public void a1_loom_2_is_available_as_stream()
        // {
        //     // -------------
        //     
        //     using var s1 = _tools.Strand_ANumber(1);
        //     using var s2 = _tools.Strand_AName("buffy");
        //     
        //     using var loom = _rzeka.Loom<ANumber, AName, UserData>(
        //         who: this,
        //         spell: source => source
        //             .Select(glyph =>
        //             {
        //                 return new UserData(glyph.Two.Name, "Cancer", glyph.One.Number);
        //             }));
        //     
        //     Assert.AreEqual(true, _library.IsStreamActive<UserData>());
        //
        //     // -------------
        // }

        
        [Test]
        public void b1_loom_2_provides_mana_to_weave()
        {
            // -------------
            
            using var s1 = _tools.Strand_ANumber_Synchronous(1);
            using var s2 = _tools.Strand_AName_Synchronous("buffy");
            
            using var loom = _rzeka.Loom<ANumber, AName, UserData>(
                who: this,
                spell: source => source
                    .Select(glyph =>
                    {
                        return new UserData(glyph.Two.Name, "Cancer", glyph.One.Number);
                    }));
            
            using var w1 = _tools.Weave_UserData(out var weave);

            Assert.AreEqual(true, weave.ThisAsBinding.HasMana);

            // -------------
        }
        
        [Test]
        public void b2_loom_2_provides_mana_to_weave_wascast()
        {
            // -------------
            
            // azsf
            
            using var s1 = _tools.Strand_ANumber_Synchronous(1);
            using var s2 = _tools.Strand_AName_Synchronous("buffy");
            
            using var loom = _rzeka.Loom<ANumber, AName, UserData>(
                who: this,
                spell: source => source
                    .Select(glyph =>
                    {
                        return new UserData(glyph.Two.Name, "Cancer", glyph.One.Number);
                    }));
            
            using var w1 = _tools.Weave_UserData(out var weave);

            Assert.AreEqual(true, weave.ThisAsBinding.HasMana);

            // -------------
        }
        
        [Test]
        public void b3_loom_3_provides_mana_to_weave()
        {
            // -------------
            
            using var s1 = _tools.Strand_ArbitraryMatter1("xander");
            using var s2 = _tools.Strand_ArbitraryMatter2("scorpio");
            using var s3 = _tools.Strand_ANumber_Synchronous(1);

            using var loom = _rzeka.Loom<ArbitraryMatter1, ArbitraryMatter2, ANumber, UserData>(
                who: this,
                spell: source => source
                    .Select(glyph =>
                    {
                        return new UserData(glyph.One.Text, glyph.Two.Text, glyph.Three.Number);
                    }));
            
            using var w1 = _tools.Weave_UserData(out var weave);

            Assert.AreEqual(true, weave.ThisAsBinding.HasMana);

            // -------------
        }
        
        [Test]
        public void b4_loom_3_provides_mana_to_weave_wascast()
        {
            // -------------
            
            using var s1 = _tools.Strand_ArbitraryMatter1("xander");
            using var s2 = _tools.Strand_ArbitraryMatter2("scorpio");
            using var s3 = _tools.Strand_ANumber_Synchronous(1);

            using var loom = _rzeka.Loom<ArbitraryMatter1, ArbitraryMatter2, ANumber, UserData>(
                who: this,
                spell: source => source
                    .Select(glyph =>
                    {
                        return new UserData(glyph.One.Text, glyph.Two.Text, glyph.Three.Number);
                    }));
            
            using var w1 = _tools.Weave_UserData(out var weave);

            Assert.AreEqual(true, weave.ThisAsBinding.HasMana);

            // -------------
        }
    }
}
