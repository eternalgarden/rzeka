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
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.CLibrary
{
    public class Library_03_Stream_Unegistration
    {
        ITestableRzeka _rzeka;
        Rzeka.Library _library;
        TestTools _tools;
    
        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            _rzeka = new SpringRiver();
            _library = _rzeka.Library;
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

        // [Test] public void a1_unregistered_strand_deactivates_stream()
        // {
        //     // -------------
        //
        //     IDisposable d1 = _tools.Strand_ANumber(1);
        //     
        //     d1.Dispose();
        //
        //     TestTools.AssertEqual(false, _library.IsStreamActive<ANumber>());
        //
        //     // -------------
        // }
        
        // [Test] public void a2_unregistered_loom_deactivates_stream()
        // {
        //     // -------------
        //
        //     using var d1 = _tools.Strand_ANumber(1);
        //     IDisposable d2 = _tools.Loom_ANumber_To_AName(out _);
        //     
        //     d2.Dispose();
        //
        //     TestTools.AssertEqual(false, _library.IsStreamActive<AName>());
        //
        //     // -------------
        // }
        
        // [Test] public void b_registered_loom_that_lost_mana_is_no_longer_a_stream()
        // {
        //     // -------------
        //
        //     IDisposable d1 = _tools.Strand_ANumber(1);
        //     using var d2 = _tools.Loom_ANumber_To_AName(out _);
        //     
        //     d1.Dispose();
        //
        //     TestTools.AssertEqual(false, _library.IsStreamActive<AName>());
        //
        //     // -------------
        // }
    }
}
