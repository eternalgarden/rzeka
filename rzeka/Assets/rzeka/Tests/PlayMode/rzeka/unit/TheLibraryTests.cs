/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace Rzeka.Tests.Unit
{
    public class TheLibraryTests : TestBase
    {
        // -------------
        
        static IRzeka Rzeka = new RzekaXOXO();

        [UnitySetUp]
        public override IEnumerator Setup()
        {
            // -------------

            base.Setup();
            yield return null;

            // -------------
        }

        [UnityTearDown]
        public override IEnumerator Teardown()
        {
            // -------------

            base.Teardown();
            yield return null;

            // -------------
        }

        // [Test]
        // [TestCase(new ConjuringScroll<ANumber>(null, null, Rzeka.TheLibrary, Rzeka.Eris))]
        // public void CastConjuring_SavesActiveConjuring(IConjuringScroll conjuringScroll)
        // {
        //     ConjuringScroll<ANumber> Scroll = ;

        //     Rzeka.TheLibrary.CastConjuring(Scroll);

        //     bool hasANumberActiveConjuring = Rzeka.TheLibrary.TryGetActiveBindings<ANumber>(out _);

        //     AssertEqual(true, hasANumberActiveConjuring);
        // }

        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 05 November 2022 🌊 */