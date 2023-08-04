/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using NUnit.Framework;
using System.Collections;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Library
{
    public class Library_01_Basics
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
        
        [Test]
        public void a_strand_spell_counts_as_stream()
        {
            // -------------

            using var d2 = _tools.Strand_ANumber_Synchronous(1);
            
            Assert.IsTrue(_library.StreamsCount == 1);

            // -------------
        }
        
        [Test]
        public void b_does_stream_exist()
        {
            // -------------

            using var d2 = _tools.Strand_ANumber_Synchronous(1);
            
            Assert.IsTrue(_library.WasStreamCreated<ANumber>());

            // -------------
        }
        
        // [Test]
        // public void c_is_stream_active()
        // {
        //     // -------------
        //
        //     using var d2 = _tools.Strand_ANumber(1);
        //     
        //     Assert.IsTrue(_library.IsStreamActive<ANumber>());
        //     
        //     // -------------
        // }
        
        [Test]
        public void d1_loom_is_provided_mana()
        {
            // -------------
            
            using var d1 = _tools.Strand_ANumber_Synchronous(1);
            using var d3 = _tools.Loom_ANumber_To_AName(out var loomingScroll);

            Assert.AreEqual(true, loomingScroll.ThisAsBinding.HasMana);

            // -------------
        }
        
        [Test]
        public void d2_loom_is_provided_mana_was_cast()
        {
            // -------------
            
            using var d1 = _tools.Strand_ANumber_Synchronous(1);
            using var d3 = _tools.Loom_ANumber_To_AName(out var loomingScroll);

            Assert.AreEqual(true, loomingScroll.ThisAsBinding.HasMana);

            // -------------
        }
        
        [Test]
        public void d3_loom_provides_mana_to_weave()
        {
            // -------------
            
            using var d1 = _tools.Strand_ANumber_Synchronous(1);
            using var d3 = _tools.Loom_ANumber_To_AName(out _);
            using var w1 = _tools.Weave_AName(out var weave);

            Assert.AreEqual(true, weave.ThisAsBinding.HasMana);

            // -------------
        }
        
        [Test]
        public void d4_loom_provides_mana_to_weave_wascast()
        {
            // -------------
            
            using var d1 = _tools.Strand_ANumber_Synchronous(1);
            using var d3 = _tools.Loom_ANumber_To_AName(out _);
            using var w1 = _tools.Weave_AName(out var weave);

            Assert.AreEqual(true, weave.ThisAsBinding.HasMana);

            // -------------
        }
        
        [Test]
        public void e_weave_is_provided_mana()
        {
            // -------------
            
            using var d1 = _tools.Strand_ANumber_Synchronous(1);
            using var d3 = _tools.Weave_ANumber(out var weavingScroll);

            Assert.AreEqual(true, weavingScroll.ThisAsBinding.HasMana);

            // -------------
        }
        
        [Test]
        public void e1_weave_is_provided_mana_was_cast()
        {
            // -------------
            
            using var d1 = _tools.Strand_ANumber_Synchronous(1);
            using var d3 = _tools.Weave_ANumber(out var weavingScroll);

            Assert.AreEqual(true, weavingScroll.ThisAsBinding.HasMana);

            // -------------
        }
    }
}
