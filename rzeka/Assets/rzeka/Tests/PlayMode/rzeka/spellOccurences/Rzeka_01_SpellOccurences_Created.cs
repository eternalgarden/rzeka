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
        
        [Test]
        [TestCase(SpellOccurenceCategory.Created, true)]
        [TestCase(SpellOccurenceCategory.HasMana, true)]
        [TestCase(SpellOccurenceCategory.NoMana, false)] 
        public void a1_alone_stranding_creation_occurences(SpellOccurenceCategory category, bool happened)
        {
            // -------------
            
            int count = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source.SpellSchool == SpellSchool.Stranding)
                .Subscribe(_ => count++);

            using var d2 = tools.Strand_ANumber(1);

            Assert.AreEqual(happened ? 1 : 0, count);

            // -------------
        }
        
        [Test]
        [TestCase(SpellOccurenceCategory.Created, true)]
        [TestCase(SpellOccurenceCategory.HasMana, false)]
        [TestCase(SpellOccurenceCategory.NoMana, true)] 
        public void a2_alone_loom_creation_occurences(SpellOccurenceCategory category, bool happened)
        {
            // -------------
            
            int count = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source.SpellSchool == SpellSchool.Looming)
                .Subscribe(_ => count++);

            using var d2 = tools.Loom_ANumber_To_AName(out _);

            Assert.AreEqual(happened ? 1 : 0, count);

            // -------------
        }
        
        [Test]
        [TestCase(SpellOccurenceCategory.Created, true)]
        [TestCase(SpellOccurenceCategory.HasMana, false)]
        [TestCase(SpellOccurenceCategory.NoMana, true)] 
        public void a3_alone_weave_creation_occurences(SpellOccurenceCategory category, bool happened)
        {
            // -------------
            
            int count = 0;

            using var d1 = Rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source.SpellSchool == SpellSchool.Weaving)
                .Subscribe(_ => count++);

            using var d2 = tools.Weave_ANumber(out _);

            Assert.AreEqual(happened ? 1 : 0, count);

            // -------------
        }
        
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 08 November 2022 🌊 */