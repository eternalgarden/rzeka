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

namespace Rzeka.Tests.Occurences.Matter
{
    /// <summary>
    /// These are very important tests, they describe the core assumption in Rzeka,
    /// that by default Matter emissions are not stored and only the observers that were
    /// already in time of emission get to receive it.
    ///
    /// [HasState] & [HasBuffer] attributes shows means of providing state / buffer memory
    /// to a certain matter stream.
    /// </summary>
    public class Stateful_Vs_Normal_Matter_Emission_Behaviour
    {
        // -------------
        
        ITestableRzeka _rzeka;
        TestTools _test;
    
        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            _rzeka = new SpringRiver();
            _test = new TestTools(_rzeka);

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
        public void a1_strand_no_observers(MatterOccurenceCategory category, bool occured)
        {
            // -------------
            
            bool actual = false;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(occ => occ.MatterOccurenceCategory == category)
                .Subscribe(_ => actual = true);

            using var d1 = _test.Strand_Return<ArbitraryMatter1>();

            TestTools.AssertEqual(occured, actual);

            // -------------
        }
        
        [Test]
        [TestCase(MatterOccurenceCategory.Shaped, true)]
        [TestCase(MatterOccurenceCategory.Received, false)]
        public void a1b_strand_single_late_observer(MatterOccurenceCategory category, bool occured)
        {
            // -------------
            
            bool actual = false;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(occ => occ.MatterOccurenceCategory == category)
                .Subscribe(_ => actual = true);

            using var d1 = _test.Strand_Return<ArbitraryMatter1>();
            using var d2 = _test.Weave<ArbitraryMatter1>();

            TestTools.AssertEqual(occured, actual);

            // -------------
        }
        
        
        
        [Test]
        [TestCase(MatterOccurenceCategory.Shaped, true)]
        [TestCase(MatterOccurenceCategory.Received, true)]
        public void a1b_strand_single_early_observer(MatterOccurenceCategory category, bool occured)
        {
            // -------------
            
            bool actual = false;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(occ => occ.MatterOccurenceCategory == category)
                .Subscribe(_ => actual = true);

            using var d2 = _test.Weave<ArbitraryMatter1>();
            using var d1 = _test.Strand_Return<ArbitraryMatter1>();

            TestTools.AssertEqual(occured, actual);

            // -------------
        }
        
        
        [Test]
        [TestCase(MatterOccurenceCategory.Shaped, true)]
        [TestCase(MatterOccurenceCategory.Received, false)]
        public void b1_stateful_strand_no_observers(MatterOccurenceCategory category, bool occured)
        {
            // -------------
            
            bool actual = false;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(occ => occ.MatterOccurenceCategory == category)
                .Subscribe(_ => actual = true);

            using var d1 = _test.Strand_ArbitraryStatefulMatter1(1);

            TestTools.AssertEqual(occured, actual);

            // -------------
        }
        
        [Test]
        [TestCase(MatterOccurenceCategory.Shaped, true)]
        [TestCase(MatterOccurenceCategory.Received, true)]
        public void b1b_stateful_strand_single_late_observer(MatterOccurenceCategory category, bool occured)
        {
            // -------------
            
            bool actual = false;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(occ => occ.MatterOccurenceCategory == category)
                .Subscribe(_ => actual = true);

            using var d1 = _test.Strand_Return<ArbitraryStatefulMatter1>();
            using var d2 = _test.Weave<ArbitraryStatefulMatter1>();

            TestTools.AssertEqual(occured, actual);

            // -------------
        }
        
        [Test]
        [TestCase(MatterOccurenceCategory.Shaped, true)]
        [TestCase(MatterOccurenceCategory.Received, true)]
        public void b1b_stateful_strand_single_early_observer(MatterOccurenceCategory category, bool occured)
        {
            // -------------
            
            bool actual = false;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(occ => occ.MatterOccurenceCategory == category)
                .Subscribe(_ => actual = true);

            using var d2 = _test.Weave<ArbitraryStatefulMatter1>();
            using var d1 = _test.Strand_Return<ArbitraryStatefulMatter1>();

            TestTools.AssertEqual(occured, actual);

            // -------------
        }
        
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 08 November 2022 🌊 */