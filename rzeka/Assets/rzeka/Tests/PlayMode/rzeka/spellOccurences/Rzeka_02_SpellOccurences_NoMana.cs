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
        
        [Test]
        public void a0_default_mana_info_stream_value_is_created()
        {
            // -------------
            
            int occurence = 0;
        
            using var d1 = Rzeka.Eris.ManaProvideableObservable
                .Subscribe(_ => occurence++);

            Assert.AreEqual(1, occurence);
        
            // -------------
        }
        
        [Test]
        public void a1_stranding_logs_as_available_mana()
        {
            // -------------
            
            int occurence = 0;
        
            using var d1 = Rzeka.Eris.ManaProvideableObservable
                .Where(info => info.IsManaOfTypeAvailable<ANumber>())
                .Subscribe(_ => occurence++);
            
            using var d2 = tools.Strand_ANumber_Synchronous(1);

            Assert.AreEqual(1, occurence);
        
            // -------------
        }
        
        [Test]
        public void a2_looming_logs_as_available_mana()
        {
            // -------------
            
            int occurence = 0;
        
            using var d1 = Rzeka.Eris.ManaProvideableObservable
                .Where(info => info.IsManaOfTypeAvailable<AName>())
                .Subscribe(_ => occurence++);
            
            using var d2 = tools.Strand_ANumber_Synchronous(1);
            using var w1 = tools.Loom_ANumber_To_AName(out _);

            Assert.AreEqual(1, occurence);
        
            // -------------
        }
        
        [Test]
        [TestCase(SpellOccurenceCategory.Created, true)]
        [TestCase(SpellOccurenceCategory.HasMana, true)]
        [TestCase(SpellOccurenceCategory.NoMana, false)] 
        public void b1_weave_mana_from_stranding(SpellOccurenceCategory category, bool happened)
        {
            // -------------
            
            int count = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source.SpellSchool == SpellSchool.Weaving)
                .Subscribe(_ => count++);

            using var d2 = tools.Strand_ANumber_Synchronous(1);
            using var w1 = tools.Weave_ANumber();

            Assert.AreEqual(happened ? 1 : 0, count);

            // -------------
        }

        static (SpellOccurenceCategory, bool)[] b2_values = new (SpellOccurenceCategory, bool)[]
        {
            (SpellOccurenceCategory.Created, true),
            (SpellOccurenceCategory.HasMana, true),
            (SpellOccurenceCategory.NoMana, true)
        };
        
        [UnityTest]
        public IEnumerator b2_weave_mana_from_stranding_reversed([ValueSource(nameof(b2_values))] (SpellOccurenceCategory, bool) value)
        {
            // -------------
            
            int count = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == value.Item1)
                .Where(occ => occ.Source.SpellSchool == SpellSchool.Weaving)
                .Subscribe(_ => count++);

            using var w1 = tools.Weave_ANumber();
            using var d2 = tools.Strand_ANumber_Synchronous(1);

            yield return new WaitForSeconds(0.2f);

            Assert.AreEqual(value.Item2 ? 1 : 0, count);

            // -------------
        }
        
        [Test]
        [TestCase(SpellOccurenceCategory.Created, true)]
        [TestCase(SpellOccurenceCategory.HasMana, true)]
        [TestCase(SpellOccurenceCategory.NoMana, false)] 
        public void c1_loom_from_stranding(SpellOccurenceCategory category, bool happened)
        {
            // -------------
            
            int count = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source.SpellSchool == SpellSchool.Looming)
                .Subscribe(_ => count++);

            using var d2 = tools.Strand_ANumber_Synchronous(1);
            using var w1 = tools.Loom_ANumber_To_AName(out _);

            Assert.AreEqual(happened ? 1 : 0, count);

            // -------------
        }
        
        
        [Test]
        [TestCase(SpellOccurenceCategory.Created, true)]
        [TestCase(SpellOccurenceCategory.HasMana, true)]
        [TestCase(SpellOccurenceCategory.NoMana, true)] 
        public void c2_loom_from_stranding_reverse(SpellOccurenceCategory category, bool happened)
        {
            // -------------
            
            int count = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source.SpellSchool == SpellSchool.Looming)
                .Subscribe(_ => count++);

            using var w1 = tools.Loom_ANumber_To_AName(out _);
            using var d2 = tools.Strand_ANumber_Synchronous(1);

            Assert.AreEqual(happened ? 1 : 0, count);

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
        
        [Test]
        [TestCase(SpellOccurenceCategory.HasMana)]
        [TestCase(SpellOccurenceCategory.NoMana)]
        public void d2_NoMana_Cast_Loom(SpellOccurenceCategory category)
        {
            // -------------
            
            bool occured = false;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source.SpellSchool is SpellSchool.Looming)
                .Subscribe(_ => occured = true);

            var p1 = tools.Strand_ANumber_Synchronous(1);
            
            using var w1 = tools.Loom_ANumber_To_AName(out _);

            p1.Dispose();

            Assert.AreEqual(true, occured);

            // -------------
        }
        
        #endregion // ---------------------------------- NoMana -------------------------
        
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 08 November 2022 🌊 */