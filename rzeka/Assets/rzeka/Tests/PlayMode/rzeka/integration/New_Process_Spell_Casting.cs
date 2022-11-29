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

namespace Rzeka.Tests.Integration
{
    public class New_Process_Spell_Casting
    {
        // -------------
        
        ITestableRzeka Rzeka;
        TestTools tools;
    
        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            Rzeka = new SpringRiver();
            tools = new TestTools(Rzeka);

            yield return null;

            // -------------
        }

        [UnityTearDown]
        public virtual IEnumerator Teardown()
        {
            // -------------

            Rzeka.Dispose();

            yield return null;

            // -------------
        }

        //
        // ⛺ ─── Casting ───────────────────────────────────────────────────
        //
        #region Casting

        [UnityTest]
        public IEnumerator b1_2_Cast_Plucking()
        {
            // -------------
            
            using var d2 = tools.Pluck_ANumber(out ConjuringScroll<ANumber> scroll);

            Assert.AreEqual(true, (scroll as TScrollBase).WasCast);

            yield return null;

            // -------------
        }
        
        [UnityTest]
        public IEnumerator b1_2_Cast_Plucking_Logged()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Stranding)
                .Subscribe(_ => occurence++);

            using var d2 = tools.Pluck_ANumber(1);

            Assert.AreEqual(1, occurence);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b2_1_Cast_Looming()
        {
            // -------------
            
            using var d2 = tools.Pluck_AName("Agent Cooper");
            using var d3 = tools.Loom_AName_To_UserData(out LoomingScroll_1<AName,UserData> scroll);

            Assert.AreEqual(true, scroll.WasCast);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b2_2_Cast_Looming_Logged()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Looming)
                .Subscribe(_ => occurence++);

            using var d2 = tools.Pluck_AName("Agent Cooper");
            using var d3 = tools.Loom_AName_To_UserData(out _);

            Assert.AreEqual(1, occurence);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b2_3_Cast_Looming_FromAnotherLoom()
        {
            // -------------
            
            using var d2 = tools.Pluck_ANumber(1);
            using var d3 = tools.Loom_ANumber_To_AName(out _);
            using var d4 = tools.Loom_AName_To_UserData(out LoomingScroll_1<AName,UserData> scroll);

            Assert.AreEqual(true, scroll.WasCast);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b2_4_Cast_Looming_Logged_FromAnotherLoom()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Looming)
                .Subscribe(_ => occurence++);
            
            using var d2 = tools.Pluck_ANumber(1);
            using var d3 = tools.Loom_ANumber_To_AName(out _);
            using var d4 = tools.Loom_AName_To_UserData(out LoomingScroll_1<AName,UserData> scroll);

            Assert.AreEqual(2, occurence); // since there are two looms that we expect to be cast

            yield return null;

            // -------------
        }

        
        //
        // ⛺ ─── Flipped order ───────────────────────────────────────────────────
        //
        #region Flipped order
        
        [UnityTest]
        public IEnumerator b2_Flipped_1_Cast_Looming()
        {
            // -------------
            
            using var d3 = tools.Loom_AName_To_UserData(out LoomingScroll_1<AName,UserData> scroll);
            using var d2 = tools.Pluck_AName("Agent Cooper");

            Assert.AreEqual(true, scroll.WasCast);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b2_Flipped_2_Cast_Looming_Logged()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Looming)
                .Subscribe(_ => occurence++);

            using var d3 = tools.Loom_AName_To_UserData(out _);
            using var d2 = tools.Pluck_AName("Agent Cooper");

            Assert.AreEqual(1, occurence);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b2_Flipped_3_Cast_Looming_FromAnotherLoom()
        {
            // -------------
            
            using var d4 = tools.Loom_AName_To_UserData(out LoomingScroll_1<AName,UserData> scroll);
            using var d3 = tools.Loom_ANumber_To_AName(out _);
            using var d2 = tools.Pluck_ANumber(1);

            Assert.AreEqual(true, scroll.WasCast);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b2_Flipped_4_Cast_Looming_Logged_FromAnotherLoom()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Looming)
                .Subscribe(_ => occurence++);
            
            using var d4 = tools.Loom_AName_To_UserData(out LoomingScroll_1<AName,UserData> scroll);
            using var d3 = tools.Loom_ANumber_To_AName(out _);
            using var d2 = tools.Pluck_ANumber(1);

            Assert.AreEqual(2, occurence);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b2_Flipped_3_Cast_Looming_FromAnotherLoom_v2()
        {
            // -------------
            
            // notice just d3 and d4 are flipped here
            using var d3 = tools.Loom_ANumber_To_AName(out _);
            using var d4 = tools.Loom_AName_To_UserData(out LoomingScroll_1<AName,UserData> scroll);
            using var d2 = tools.Pluck_ANumber(1);

            Assert.AreEqual(true, scroll.WasCast);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b2_Flipped_4_Cast_Looming_Logged_FromAnotherLoom_v2()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Looming)
                .Subscribe(_ => occurence++);
            
            // notice just d3 and d4 are flipped here
            using var d3 = tools.Loom_ANumber_To_AName(out _);
            using var d4 = tools.Loom_AName_To_UserData(out LoomingScroll_1<AName,UserData> scroll);
            using var d2 = tools.Pluck_ANumber(1);

            Assert.AreEqual(2, occurence);

            yield return null;

            // -------------
        }
        
        #endregion // ---------------------------------- Flipped order -------------------------

        
        //
        // ⛺ ─── Weave asting ───────────────────────────────────────────────────
        //
        #region Weave Casting
        
        [UnityTest]
        public IEnumerator b3_Cast_Weave()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Weaving)
                .Subscribe(_ => occurence++);

            using var d2 = tools.Pluck_ANumber(1);
            using var d3 = tools.Weave_ANumber();

            Assert.AreEqual(1, occurence);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b3b_Cast_Weave_FlippedOrder()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Weaving)
                .Subscribe(_ => occurence++);

            using var d3 = tools.Weave_ANumber();
            using var d2 = tools.Pluck_ANumber(1);

            Assert.AreEqual(1, occurence);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b4_Cast_Weave_Logged_FromLoom()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Weaving)
                .Subscribe(_ => occurence++);

            using var d2 = tools.Pluck_ANumber(1);
            using var d3 = tools.Weave_AName();
            using var d4 = tools.Loom_ANumber_To_AName(out _);

            Assert.AreEqual(1, occurence);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b4b_Cast_Weave_Logged_FromLoom_FlippedOrder()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Weaving)
                .Subscribe(_ => occurence++);

            using var d3 = tools.Weave_AName();
            using var d4 = tools.Loom_ANumber_To_AName(out _);
            using var d2 = tools.Pluck_ANumber(1);

            Assert.AreEqual(1, occurence);

            yield return null;

            // -------------
        }
        
        #endregion // ---------------------------------- Weave asting -------------------------

        #endregion // ---------------------------------- Casting -------------------------
        
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 08 November 2022 🌊 */