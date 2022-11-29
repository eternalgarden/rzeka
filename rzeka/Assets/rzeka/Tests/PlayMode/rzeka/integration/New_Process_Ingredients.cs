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

namespace Rzeka.Tests.Integration
{
    public class New_Process_Ingredients
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
        // ⛺ ─── Ingredient Checks ───────────────────────────────────────────────────
        //
        #region Ingredient Checks

        [UnityTest] // TODO this is an example of ingredient counting test, there should be those for each possible case
        public IEnumerator a1_Cast_Weave_Has_Single_Ingredient()
        {
            // -------------

            using var d2 = tools.Pluck_ANumber(1);
            using var d3 = tools.Weave_ANumber(out AlteringScroll<ANumber> scroll);

            TBindingScroll binding = scroll as TBindingScroll;

            Assert.AreEqual(1, 
                binding.GetIngredients<ANumber>().Length);

            yield return null;

            // -------------
        }

        [UnityTest] // TODO possibly flipped variants for others aswell
        public IEnumerator a11_Cast_Weave_Has_Single_Ingredient_Flipped()
        {
            // -------------

            using var d3 = tools.Weave_ANumber(out AlteringScroll<ANumber> scroll);
            using var d2 = tools.Pluck_ANumber(1);

            TBindingScroll binding = scroll as TBindingScroll;

            Assert.AreEqual(1, 
                binding.GetIngredients<ANumber>().Length);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a2_Cast_Weave_Has_Expected_Ingredient()
        {
            // -------------

            using var d2 = tools.Pluck_ANumber(out ConjuringScroll<ANumber> conjuring, 1);
            using var d3 = tools.Weave_ANumber(out AlteringScroll<ANumber> scroll);

            TBindingScroll binding = scroll as TBindingScroll;

            Assert.AreEqual(true, 
                binding.GetIngredients<ANumber>()[0] == conjuring);

            yield return null;

            // -------------
        }

        [UnityTest] 
        public IEnumerator b1_Blocked_Loom_Does_Not_Count_As_Ingredient()
        {
            // -------------

            using var d2 = tools.Loom_ANumber_To_AName(out _);
            using var d3 = tools.Weave_AName(out AlteringScroll<AName> scroll);

            TBindingScroll binding = scroll as TBindingScroll;

            Assert.AreEqual(0, binding.GetIngredients<AName>().Length);

            yield return null;

            // -------------
        }
        
        #endregion // ---------------------------------- Ingredient Checks -------------------------
        
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 08 November 2022 🌊 */