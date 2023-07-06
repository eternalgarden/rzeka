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
    public class Library_02_Stream_Registration
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

        [Test] public void a_is_counted_as_library_stream_strand()
        {
            // -------------

            using var d1 = _tools.Strand_ANumber_Synchronous(1);

            TestTools.AssertEqual(1, _library.StreamsCount);

            // -------------
        }
        
        // [Test] public void a_is_active_stream_strand()
        // {
        //     // -------------
        //
        //     using var d1 = _tools.Strand_ANumber(1);
        //
        //     TestTools.AssertEqual(true, _library.IsStreamActive<ANumber>());
        //
        //     // -------------
        // }
        
        [Test] public void b1_nomana_loom_2_creates_both_streams_immediately()
        {
            // -------------

            using var d1 = _tools.Loom_ANumber_To_AName(out _);

            TestTools.AssertEqual(true, _library.WasStreamCreated<ANumber>());

            // -------------
        }
        
        // [Test] public void b2_nomana_loom_doesnt_count_as_an_active_stream()
        // {
        //     // -------------
        //
        //     using var d1 = _tools.Loom_ANumber_To_AName(out _);
        //
        //     TestTools.AssertEqual(false, _library.IsStreamActive<AName>());
        //
        //     // -------------
        // }
        //
        // [Test] public void b3_cast_loom_registers_as_an_active_stream()
        // {
        //     // -------------
        //
        //     using var d1 = _tools.Strand_ANumber(1);            
        //     using var d2 = _tools.Loom_ANumber_To_AName(out _);
        //
        //     TestTools.AssertEqual(true, _library.IsStreamActive<AName>());
        //
        //     // -------------
        // }
        
        [Test] public void b4_one_strand_one_looom_gives_two_streams()
        {
            // -------------

            using var d1 = _tools.Strand_ANumber_Synchronous(1);            
            using var d2 = _tools.Loom_ANumber_To_AName(out _);

            TestTools.AssertEqual(2, _library.StreamsCount);

            // -------------
        }
        
        [Test] public void c1_nomana_weave_creates_needed_streams_immediately()
        {
            // -------------

            using var d1 = _tools.Weave_AName(out _);

            TestTools.AssertEqual(true, _library.WasStreamCreated<AName>());

            // -------------
        }
        
        // [Test] public void c2_such_created_stream_is_inactive()
        // {
        //     // -------------
        //
        //     using var d1 = _tools.Weave_AName(out _);
        //
        //     TestTools.AssertEqual(false, _library.IsStreamActive<AName>());
        //
        //     // -------------
        // }
    }
}
