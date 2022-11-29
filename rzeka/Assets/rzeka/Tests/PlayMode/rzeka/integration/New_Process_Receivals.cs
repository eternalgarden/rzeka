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
    public class New_Process_Receivals
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
        // ⛺ ─── Receivals ───────────────────────────────────────────────────
        //
        #region Receivals
        
        [UnityTest]
        public IEnumerator a1_Weave_Got_Plucking()
        {
            // -------------
            
            int occurence = 0;

            using var d2 = tools.Pluck_ANumber(1);
            using var d3 = tools.Weave_ANumber(i => {
                if(i.Number is 1) occurence++;
            });

            Assert.AreEqual(1, occurence);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a2_Weave_Got_Plucking_Multiple()
        {
            // -------------
            
            int occurence = 0;

            using var d2 = tools.Pluck_ANumber(1,2,3);
            using var d3 = tools.Weave_ANumber(i => {
                if(i.Number is 1 or 2 or 3) occurence++;
            });

            Assert.AreEqual(3, occurence);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a3_Weave_Got_Looming()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = tools.Pluck_ANumber(1,2,3);
            using var d2 = tools.Loom_ANumber_To_AName(out _);
            using var d3 = tools.Weave_AName(i => {
                if(i.Name is "1" or "2" or "3") occurence++;
            });

            Assert.AreEqual(3, occurence);

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a4_Weave_Got_Double_Looming()
        {
            // -------------
            
            int occurence = 0;

            using var d1 = tools.Pluck_ANumber(1,2,3);
            using var d2 = tools.Loom_ANumber_To_AName(out _);
            using var d3 = tools.Loom_AName_To_UserData(out _);
            using var d4 = tools.Weave_UserData(i => {
                if(i.Name is "1" or "2" or "3") occurence++;
            });

            Assert.AreEqual(3, occurence);

            yield return null;

            // -------------
        }
        
        #endregion // ---------------------------------- Receivals -------------------------
    
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 28 November 2022 🌊 */