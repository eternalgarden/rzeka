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
    public class Rzeka_02_SpellOccurences_NoMana
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
            yield return null;

            Assert.AreEqual(2, occurence);


            // -------------
        }

        [Test]
        [TestCase(SpellOccurenceCategory.Cast)]
        [TestCase(SpellOccurenceCategory.NoMana)]
        public void c2_NoMana_Cast_Weave(SpellOccurenceCategory category)
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Weaving)
                .Subscribe(_ => occurence++);

            var p1 = tools.Strand_ANumber(1);
            
            using var w1 = tools.Weave_ANumber();

            p1.Dispose();

            Assert.AreEqual(1, occurence);

            // -------------
        }
        
        [UnityTest]
        public IEnumerator d1_NoMana_Created_Loom()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Created)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Looming)
                .Subscribe(_ => occurence++);

            using var d2 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.NoMana)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Looming)
                .Subscribe(_ => occurence++);

            using var w1 = tools.Loom_ANumber_To_AName(out _);

            // * this is due no mana is cast with 0.3 second delay
            yield return null;

            Assert.AreEqual(2, occurence);


            // -------------
        }
        
        [UnityTest]
        public IEnumerator d2_NoMana_Cast_Loom()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Cast)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Looming)
                .Subscribe(_ => occurence++);

            using var d2 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.NoMana)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Looming)
                .Subscribe(_ => occurence++);

            var p1 = tools.Strand_ANumber(1);
            
            using var w1 = tools.Loom_ANumber_To_AName(out _);

            p1.Dispose();

            yield return null;

            Assert.AreEqual(2, occurence);

            // -------------
        }
        
        #endregion // ---------------------------------- NoMana -------------------------
        
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 08 November 2022 🌊 */