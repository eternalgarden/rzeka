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

namespace Rzeka.Tests.Occurences.Spell
{
    public class Rzeka_04_SpellOccurences_Forgotten
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
        // ⛺ ─── Forgetting ───────────────────────────────────────────────────
        //
        #region Forgetting
        
        [UnityTest]
        public IEnumerator d1_Forgotten_Plucking()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.HasMana)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Stranding)
                .Subscribe(_ => occurence++);

            using var d2 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory is SpellOccurenceCategory.Forgotten)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Stranding)
                .Subscribe(_ => occurence++);

            var p1 = tools.Strand_ANumber_Synchronous(1);

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