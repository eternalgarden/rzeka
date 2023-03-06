/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using NUnit.Framework;
using System;
using System.Collections;
using System.Reactive.Linq;
using UnityEngine.TestTools;

namespace Rzeka.Tests.ASpellOccurences
{
    public class Rzeka_01_SpellOccurences_Created
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
        
        [Test]
        [TestCase(SpellOccurenceCategory.Created, true)]
        [TestCase(SpellOccurenceCategory.HasMana, true)]
        [TestCase(SpellOccurenceCategory.NoMana, false)] 
        public void a1_alone_stranding_creation_occurences(SpellOccurenceCategory category, bool happened)
        {
            // -------------
            
            int count = 0;

            using var d1 = _rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source.SpellSchool == SpellSchool.Stranding)
                .Subscribe(_ => count++);

            using var d2 = _tools.Strand_ANumber_Synchronous(1);

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

            using var d1 = _rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source.SpellSchool == SpellSchool.Looming)
                .Subscribe(_ => count++);

            using var d2 = _tools.Loom_ANumber_To_AName(out _);

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

            using var d1 = _rzeka.Eris.SpellOccurences
                .Where(occ => occ.SpellOccurenceCategory == category)
                .Where(occ => occ.Source.SpellSchool == SpellSchool.Weaving)
                .Subscribe(_ => count++);

            using var d2 = _tools.Weave_ANumber(out _);

            Assert.AreEqual(happened ? 1 : 0, count);

            // -------------
        }
        
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 08 November 2022 🌊 */