/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Matter.Default
{
    public class Matter_Default_02_Circumstances
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

        [UnityTest]
        public IEnumerator a_are_any_circumstances_set()
        {
            using var l1 = _tools.Loom_ANumber_To_AName(out _);

            bool areAny = false;
            
            using var w2 = _tools.Weave_AName(name => {
                areAny = name.Circumstances is not null && name.Circumstances.Length > 0;
            });
            
            using var p1 = _tools.Strand_ANumber_Synchronous(1);

            yield return null;

            TestTools.AssertEqual(true, areAny);
        }

        [UnityTest]
        public IEnumerator b_are_two_different_weavings_receiving_same_conjurring()
        {
            using var l1 = _tools.Loom_ANumber_To_AName(out _);

            Guid num1Guid = Guid.NewGuid(); // so we can be sure they will be starting out different
            Guid num2Guid = Guid.NewGuid(); // ^

            using var w1 = _tools.Weave_ANumber(num => num1Guid = num.Guid);
            using var w2 = _tools.Weave_ANumber(num => num2Guid = num.Guid);
            
            using var p1 = _tools.Strand_ANumber_Synchronous(1);

            yield return null;

            TestTools.AssertEqual(num1Guid, num2Guid);
        }

        [UnityTest]
        public IEnumerator c_are_guids_same_for_matter_received_and_also_used_as_circumstance()
        {
            using var l1 = _tools.Loom_ANumber_To_AName(out _);

            Guid numberGuid =           Guid.NewGuid(); // so we can be sure they will be starting out different
            Guid nameCircumstanceGuid = Guid.NewGuid();

            using var w1 = _tools.Weave_ANumber(num => numberGuid = num.Guid);
            using var w2 = _tools.Weave_AName(name => nameCircumstanceGuid = name.Circumstances[0]);
            
            using var p1 = _tools.Strand_ANumber_Synchronous(1); // * both will be separately woven

            yield return null;

            TestTools.AssertEqual(numberGuid, nameCircumstanceGuid);
        }
        
    
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 02 December 2022 🌊 */