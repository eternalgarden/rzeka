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
    public class New_Process
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
        // ⛺ ─── Creation ───────────────────────────────────────────────────
        //
        #region Creation
        
        [UnityTest]
        public IEnumerator a1_Created_Pluck_Logged()
        {
            // -------------
            
            int createdOccurenceNoted = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Created)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Stranding)
                .Where(occ => (occ.Source as IConjuringSpell).ConjuredType == typeof(ANumber))
                .Subscribe(_ => createdOccurenceNoted++);

            using var d2 = tools.Pluck_ANumber(1);

            Assert.AreEqual(1, createdOccurenceNoted);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a1_Created_Pluck_NotNull()
        {
            // -------------
            
            using var d2 = tools.Pluck_ANumber(out var scroll);

            Assert.NotNull(scroll);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a2_Created_Loom_Logged()
        {
            // -------------
            
            int createdOccurenceNoted = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Created)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Looming)
                .Where(occ => (occ.Source as IConjuringSpell).ConjuredType == typeof(UserData))
                .Subscribe(_ => createdOccurenceNoted++);

            using var d2 = tools.Loom_AName_To_UserData(out _);

            Assert.AreEqual(1, createdOccurenceNoted);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a2_Created_Loom_NotNull()
        {
            // -------------
            
            using var d2 = tools.Loom_AName_To_UserData(out var scroll);

            Assert.NotNull(scroll);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a3_Created_Weave_Logged()
        {
            // -------------
            
            int createdOccurenceNoted = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Created)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Weaving)
                .Where(occ => (occ.Source as TWeavingSpell).WouldPossiblyLike<ANumber>())
                .Subscribe(_ => createdOccurenceNoted++);

            using var d2 = tools.Weave_ANumber();

            Assert.AreEqual(1, createdOccurenceNoted);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a3_Created_Weave_NotNull()
        {
            // -------------
            
            using var d2 = tools.Weave_ANumber(out var scroll);

            Assert.NotNull(scroll);

            yield return null;

            // -------------
        }
        
        #endregion // ---------------------------------- Creation -------------------------
        
        //
        // ⛺ ─── NoMana ───────────────────────────────────────────────────
        //
        #region NoMana
        
        [UnityTest]
        public IEnumerator c1_NoMana_Created_Weave()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Created)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Weaving)
                .Subscribe(_ => occurence++);

            using var d2 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.NoMana)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Weaving)
                .Subscribe(_ => occurence++);

            using var w1 = tools.Weave_ANumber();

            // * this is due no mana is cast with 0.3 second delay
            yield return new WaitForSeconds(0.4f);

            Assert.AreEqual(2, occurence);


            // -------------
        }

        [UnityTest]
        public IEnumerator c2_NoMana_Cast_Weave()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Weaving)
                .Subscribe(_ => occurence++);

            using var d2 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.NoMana)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Weaving)
                .Subscribe(_ => occurence++);

            var p1 = tools.Pluck_ANumber(1);
            
            using var w1 = tools.Weave_ANumber();

            yield return null;

            p1.Dispose();

            Assert.AreEqual(2, occurence);


            // -------------
        }
        
        #endregion // ---------------------------------- NoMana -------------------------

        
        //
        // ⛺ ─── Forgetting ───────────────────────────────────────────────────
        //
        #region Forgetting
        
        [UnityTest]
        public IEnumerator d1_Forgotten_Plucking()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Stranding)
                .Subscribe(_ => occurence++);

            using var d2 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Forgotten)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Stranding)
                .Subscribe(_ => occurence++);

            var p1 = tools.Pluck_ANumber(1);

            p1.Dispose();

            yield return null;

            Assert.AreEqual(2, occurence);


            // -------------
        }

        // todo loom
        

        [UnityTest]
        public IEnumerator d3_Forgotten_Weaving()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Created)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Weaving)
                .Subscribe(_ => occurence++);

            using var d2 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Forgotten)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Weaving)
                .Subscribe(_ => occurence++);

            var w1 = tools.Weave_ANumber();

            w1.Dispose();

            yield return null;

            Assert.AreEqual(2, occurence);


            // -------------
        }
        
        #endregion // ---------------------------------- Forgetting -------------------------
        
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 08 November 2022 🌊 */