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
    public class MatterOccurences_02_Overloads
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
        [TestCase(MatterOccurenceCategory.Shaped, typeof(ANumber))]
        [TestCase(MatterOccurenceCategory.Received, typeof(ANumber))]
        [TestCase(MatterOccurenceCategory.Shaped, typeof(AName))]
        [TestCase(MatterOccurenceCategory.Received, typeof(AName))]
        [TestCase(MatterOccurenceCategory.Shaped, typeof(UserData))]
        [TestCase(MatterOccurenceCategory.Received, typeof(UserData))]
        public void a1_loom_2_shaped_receiveable_matter(MatterOccurenceCategory category, Type type)
        {
            // -------------
            
            bool registered = false;
            
            using var m1 = _rzeka.Eris.MatterOccurences
                .Where(m => m.MatterOccurenceCategory == category)
                .Where(m => m.Matter.GetType() == type)
                .Subscribe(_ => registered = true);

            using var s1 = _tools.Strand_ANumber(1);
            using var s2 = _tools.Strand_AName("fluff");

            using var loom = _rzeka.Loom<ANumber, AName, UserData>(
                who: this,
                spell: source => source
                    .Select(glyph =>
                    {
                        return new UserData(glyph.Two.Name, "Cancer", glyph.One.Number);
                    }));

            using var d1 = _tools.Weave_UserData();

            TestTools.AssertEqual(true, registered);

            // -------------
        }
    }
}