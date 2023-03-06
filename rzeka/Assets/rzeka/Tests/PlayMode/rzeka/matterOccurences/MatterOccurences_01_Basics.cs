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

namespace Rzeka.Tests.BMatterOccurences
{
    public class MatterOccurences_01_Basics
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
        [TestCase(MatterOccurenceCategory.Shaped, true)]
        [TestCase(MatterOccurenceCategory.Received, false)]
        public void a1_strand_alone_matter_occurence_emission(MatterOccurenceCategory category, bool occured)
        {
            // -------------
            
            bool actual = false;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(occ => occ.MatterOccurenceCategory == category)
                .Subscribe(_ => actual = true);

            using var d1 = _tools.Strand_ANumber_Synchronous(1);

            TestTools.AssertEqual(occured, actual);

            // -------------
        }
        
        [Test]
        [TestCase(typeof(ANumber), MatterOccurenceCategory.Shaped, true)]
        [TestCase(typeof(ANumber), MatterOccurenceCategory.Received, true)]
        [TestCase(typeof(AName), MatterOccurenceCategory.Shaped, true)]
        [TestCase(typeof(AName), MatterOccurenceCategory.Received, false)]
        public void a2_strand_and_loom_matter_occurence_emission(Type matterType, MatterOccurenceCategory category, bool occured)
        {
            // -------------
            
            bool actual = false;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(occ => occ.Matter.GetType() == matterType)
                .Where(occ => occ.MatterOccurenceCategory == category)
                .Subscribe(_ => actual = true);

            using var d1 = _tools.Strand_ANumber_Synchronous(1);
            using var d2 = _tools.Loom_ANumber_To_AName(out _);

            TestTools.AssertEqual(occured, actual);

            // -------------
        }
        
        [Test]
        [TestCase(typeof(ANumber), MatterOccurenceCategory.Shaped, true)]
        [TestCase(typeof(ANumber), MatterOccurenceCategory.Received, true)]
        [TestCase(typeof(AName), MatterOccurenceCategory.Shaped, true)]
        [TestCase(typeof(AName), MatterOccurenceCategory.Received, true)]
        public void a3_strand_loom_and_weave_matter_occurence_emission(Type matterType, MatterOccurenceCategory category, bool occured)
        {
            // -------------
            
            bool actual = false;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(occ => occ.Matter.GetType() == matterType)
                .Where(occ => occ.MatterOccurenceCategory == category)
                .Subscribe(_ => actual = true);

            using var d1 = _tools.Strand_ANumber_Synchronous(1);
            using var d2 = _tools.Loom_ANumber_To_AName(out _);
            using var w1 = _tools.Weave_AName(out _);

            TestTools.AssertEqual(occured, actual);

            // -------------
        }
        
        [Test]
        [TestCase(new int[] {1,2,3}, typeof(ANumber), MatterOccurenceCategory.Shaped, true)]
        [TestCase(new int[] {1,2,3}, typeof(ANumber), MatterOccurenceCategory.Received, true)]
        [TestCase(new int[] {1,2,3}, typeof(AName), MatterOccurenceCategory.Shaped, true)]
        [TestCase(new int[] {1,2,3}, typeof(AName), MatterOccurenceCategory.Received, true)]
        public void a4_strand_loom_and_weave_matter_occurence_emission_multiple_emissions(int[] values, Type matterType, MatterOccurenceCategory category, bool occured)
        {
            // -------------
            
            int count = 0;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(occ => occ.Matter.GetType() == matterType)
                .Where(occ => occ.MatterOccurenceCategory == category)
                .Subscribe(_ => count++);

            using var w1 = _tools.Weave_AName(out _);
            using var d2 = _tools.Loom_ANumber_To_AName(out _);
            using var d1 = _tools.Strand_ANumber_Synchronous(values);

            TestTools.AssertEqual(values.Length, count);

            // -------------
        }
        
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 08 November 2022 🌊 */