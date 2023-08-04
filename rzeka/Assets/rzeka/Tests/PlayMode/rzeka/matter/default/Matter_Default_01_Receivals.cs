/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Matter.Default
{
    public class Matter_Default_01_Receivals
    {
        // -------------
        
        ITestableRzeka _rzeka;
        TestTools _tools;
    
        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            _rzeka = new SpringRiver();
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
    
        //
        // ⛺ ─── Receivals ───────────────────────────────────────────────────
        //
        #region Receivals
        
        [Test]
        public void a_rzeka_does_NOT_replay_one_last_value_by_default()
        {
            // -------------
            
            bool occurence = false;

            using var d2 = _tools.Strand_ANumber_Synchronous(1);
            using var d3 = _tools.Weave_ANumber(_ => occurence = true);

            Assert.AreEqual(false, occurence);

            // -------------
        }
        
        
        [Test]
        [TestCase(new int[] {5,8})]
        [TestCase(new int[] {1,2,3})]
        public void b_early_subscriber_gets_all_the_values(int[] emitValues)
        {
            // -------------

            int receivals = 0;

            using var w1 = _tools.Weave_ANumber(_ => receivals++);
            using var s1 = _tools.Strand_ANumber_Synchronous(emitValues);

            Assert.AreEqual(emitValues.Length, receivals);

            // -------------
        }
        
        [Test]
        [TestCase(new int[] {5})]
        [TestCase(new int[] {5,8})]
        [TestCase(new int[] {1,2,3})]
        public void c_early_subscriber_gets_all_the_values_loom_version(int[] emitValues)
        {
            // -------------
            
            int receivals = 0;

            using var w1 = _tools.Weave_AName(_ => receivals++);
            using var l1 = _tools.Loom_ANumber_To_AName(out _);
            using var s1 = _tools.Strand_ANumber_Synchronous(emitValues);

            Assert.AreEqual(emitValues.Length, receivals);

            // -------------
        }
        
        // ! THESE ARE ALL OUTDATED DUE TO THIS LITTLE PARADIGM SHIFT OF NO-CACHE BY DEFAULT
        
        // [Test]
        // [TestCase(new int[] {1,2,3}, 3)]
        // [TestCase(new int[] {5}, 5)]
        // public void a1_late_subscriber_will_only_receive_last_value_by_default(int[] emitValues, int receivedValue)
        // {
        //     // -------------
        //
        //     List<int> receivednumbers = new();
        //     
        //     // notice there are two values pushed
        //     using var d2 = _tools.Strand_ANumber_Synchronous(emitValues);
        //     using var d3 = _tools.Weave_ANumber(i => {
        //         receivednumbers.Add(i.Number);
        //     });
        //
        //     bool areEqual = receivednumbers.SequenceEqual(new int[] {receivedValue});
        //
        //     Assert.AreEqual(true, areEqual, "Array: {0}", receivednumbers
        //         .Aggregate("", (s, i) => s + $"{i.ToString()},"));
        //
        //     // -------------
        // }
        //
        // [Test]
        // [TestCase(new int[] {5})]
        // [TestCase(new int[] {5,8})]
        // [TestCase(new int[] {1,2,3})]
        // public void a2_similarly_with_loom_only_last_value_is_cached_by_default(int[] emitValues)
        // {
        //     // -------------
        //     
        //     int occurence = 0;
        //
        //     using var d1 = _tools.Strand_ANumber_Synchronous(emitValues);
        //     using var d2 = _tools.Loom_ANumber_To_AName(out _);
        //     using var d3 = _tools.Weave_AName(i => {
        //         occurence++;
        //     });
        //     
        //     // 1 is expected because Weave is being registered after the Loom
        //     // and the way
        //     Assert.AreEqual(1, occurence);
        //
        //     // -------------
        // }
        //
        // [Test]
        // [TestCase(new int[] {1,2,3}, "3")]
        // [TestCase(new int[] {5}, "5")]
        // public void a3_and_it_is_obviously_the_last_value_that_was_emit(int[] emitValues, string received)
        // {
        //     // -------------
        //     
        //     string text = "";
        //
        //     using var d1 = _tools.Strand_ANumber_Synchronous(emitValues);
        //     using var d2 = _tools.Loom_ANumber_To_AName(out _);
        //     using var d3 = _tools.Weave_AName(i =>
        //     {
        //         text = i.Name;
        //     });
        //
        //     Assert.AreEqual(received, text);
        //
        //     // -------------
        // }
        //
        // [Test]
        // [TestCase(new int[] {5}, "5")]
        // [TestCase(new int[] {5,8}, "8")]
        // [TestCase(new int[] {1,2,3}, "3")]
        // public void a4_it_works_the_same_for_a_longer_spell_chain(int[] emitValues, string received)
        // {
        //     // -------------
        //     
        //     string text = "";
        //
        //     using var d1 = _tools.Strand_ANumber_Synchronous(emitValues);
        //     using var d2 = _tools.Loom_ANumber_To_AName(out _);
        //     using var d3 = _tools.Loom_AName_To_UserData(out _);
        //     using var d4 = _tools.Weave_UserData(i =>
        //     {
        //         text = i.Name;
        //     });
        //
        //     Assert.AreEqual(received, text);
        //
        //     // -------------
        // }
        //
        // [Test]
        // [TestCase(new int[] {5})]
        // [TestCase(new int[] {5,8})]
        // [TestCase(new int[] {1,2,3})]
        // public void a5_in_a_longer_chain_late_subscriber_gets_only_last_value_anyway(int[] emitValues)
        // {
        //     // -------------
        //
        //     int receivals = 0;
        //
        //     using var d1 = _tools.Strand_ANumber_Synchronous(emitValues);
        //     using var d2 = _tools.Loom_ANumber_To_AName(out _);
        //     using var d3 = _tools.Loom_AName_To_UserData(out _);
        //     using var d4 = _tools.Weave_UserData(i =>
        //     {
        //         receivals++;
        //     });
        //
        //     Assert.AreEqual(1, receivals);
        //
        //     // -------------
        // }
        
        #endregion // ---------------------------------- Receivals -------------------------
    
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 28 November 2022 🌊 */