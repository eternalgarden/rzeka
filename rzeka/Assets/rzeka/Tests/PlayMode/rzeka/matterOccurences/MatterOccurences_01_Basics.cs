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

namespace Rzeka.Tests.MatterOccurences
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
        public void a_no_weaving_no_matter_occurences_strand()
        {
            // -------------
            
            int matterOccurenceCount = 0;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Subscribe(_ => matterOccurenceCount++);

            using var d1 = _tools.Strand_ANumber(1);

            TestTools.AssertEqual(0, matterOccurenceCount);

            // -------------
        }
        
        [Test]
        public void a1_no_weaving_no_matter_occurences_loom()
        {
            // -------------
            
            int matterOccurenceCount = 0;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Subscribe(_ => matterOccurenceCount++);

            using var d1 = _tools.Strand_ANumber(1);
            using var d2 = _tools.Loom_ANumber_To_AName(out _);

            TestTools.AssertEqual(0, matterOccurenceCount);

            // -------------
        }
        
        [Test]
        public void b_single_matter_emission_two_matter_occurences()
        {
            // -------------
            
            int matterOccurenceCount = 0;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Subscribe(_ => matterOccurenceCount++);

            using var d1 = _tools.Strand_ANumber(1);
            using var d2 = _tools.Weave_ANumber(_ => {});

            TestTools.AssertEqual(2, matterOccurenceCount);

            // -------------
        }
        
        [Test]
        public void c1_matter_emit_by_stranding()
        {
            // -------------
            
            bool shaped = false;

            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(m => m.MatterOccurenceCategory == MatterOccurenceCategory.Shaped)
                .Subscribe(m => shaped = true);

            using var d1 = _tools.Strand_ANumber(1);
            using var d2 = _tools.Weave_ANumber(_ => {});

            TestTools.AssertEqual(true, shaped);

            // -------------
        }
        
        [Test]
        public void c2_emitted_matter_received_by_weaving()
        {
            // -------------
            
            bool received = false;

            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(m => m.MatterOccurenceCategory == MatterOccurenceCategory.Received)
                .Subscribe(m => received = true);

            using var d1 = _tools.Strand_ANumber(1);
            using var d2 = _tools.Weave_ANumber(_ => {});

            TestTools.AssertEqual(true, received);

            // -------------
        }
        
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 08 November 2022 🌊 */