/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Modules.EventStream.Tests
{
    public class TestPromiseEvent : PromiseEvent<int> { }

    public class PromiseTests
    {
        // -------------

        IEventStream EventStream;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // -------------

            SceneManager.LoadScene("eventStreamTestRealm", LoadSceneMode.Single);

            // Skip 2 frames to let the scene load.
            yield return null;
            yield return null;

            EventStream = GameObject.FindObjectOfType<SanctuaryEventStream>();

            yield return new WaitForSeconds(0.3f);

            // -------------
        }

        [UnityTest]
        public IEnumerator a_SanctuaryEventStreamExists()
        {
            // -------------

            // EventStream.Factory.RegisterPromise<TestPromiseEvent>(
            //     context: this,
            //     onResolved: o =>
            // )

            yield return null;

            Assert.IsTrue(EventStream != null, "Found sanctuary event stream monobehaviour.");

            // -------------
        }

        // -------------
    }
}
/* maria aurelia at 24 May 2022 🌊 */
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */