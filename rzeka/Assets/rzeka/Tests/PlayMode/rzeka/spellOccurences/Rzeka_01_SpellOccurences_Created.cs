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

namespace Rzeka.Tests.SpellOccurences
{
    public class Rzeka_01_SpellOccurences_Created
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

            using var d2 = tools.Strand_ANumber(1);

            Assert.AreEqual(1, createdOccurenceNoted);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a1_Created_Pluck_NotNull()
        {
            // -------------
            
            using var d2 = tools.Strand_ANumber(out var scroll);

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
                .Where(occ => (occ.Source as TWeavingSpell).RequiresIngredient(typeof(ANumber)))
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
        
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 08 November 2022 🌊 */