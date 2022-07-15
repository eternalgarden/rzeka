/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using NUnit.Framework;

namespace Modules.EventStream.Tests
{
    public class EventStreamIntegration
    {
        // -------------

        IEventStream eventStream;
    
        [UnitySetUp]
        public IEnumerator Setup()
        {
            // -------------
            
            SceneManager.LoadScene("eventStreamTestRealm", LoadSceneMode.Single);

            // Skip 2 frames to let the scene load.
            yield return null;
            yield return null;

            eventStream = GameObject.FindObjectOfType<SanctuaryEventStream>();

            yield return new WaitForSeconds(0.3f);
            
            // -------------
        }

        [UnityTest]
        public IEnumerator a_SanctuaryEventStreamExists()
        {
            // -------------

            // eventStream
            //     .RegisterListener<StringEvent>(
            //         onNext: e =>
            //         {
            //             Debug.Log($"<color=magenta>sdfgsd</color>");

            //             wasEditScrollRequested = true;
            //         },
            //         context: this
            //     );

            yield return null;

            Assert.IsTrue(eventStream != null, "Found sanctuary event stream monobehaviour.");

            // -------------
        }
    
        // -------------
    }
}
/* maria aurelia at 20 November 2021 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */