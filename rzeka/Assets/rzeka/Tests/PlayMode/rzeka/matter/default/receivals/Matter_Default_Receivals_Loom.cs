/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Matter.Default.Receivals
{
    public class Matter_Default_Receivals_Loom
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
    
        //
        // ⛺ ─── Receivals ───────────────────────────────────────────────────
        //
        #region Receivals
        
        #endregion // ---------------------------------- Receivals -------------------------
    
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 28 November 2022 🌊 */