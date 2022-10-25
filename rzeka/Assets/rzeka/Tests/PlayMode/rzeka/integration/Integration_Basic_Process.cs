
using NUnit.Framework;
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public class Integration_Basic_Process : IntegrationBase
    {
        [UnityTest]
        public IEnumerator a_Is_conjurable_of_UserData()
        {
            // -------------

            Q += Pluck_UserData();
            Assert.IsTrue(Rzeka.TheLibrary.IsConjurable<UserData>());

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a_Is_conjurable_of_UserWelcomingText()
        {
            // -------------

            Q += Pluck_UserData();
            Q += Loom_UserData_To_UserWelcomingText();

            Assert.IsTrue(Rzeka.TheLibrary.IsConjurable<UserWelcomingText>());

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b_Is_Weave_UserWelcomingText_Received()
        {
            // -------------

            Q += Pluck_UserData();
            Q += Loom_UserData_To_UserWelcomingText();

            bool received = false;
            
            Q += Weave_UserWelcomingText(onNext: u => received = true);

            yield return null;

            Assert.IsTrue(received);

            // -------------
        }

        [UnityTest]
        public IEnumerator b_Is_Weave_UserWelcomingText_Received_By_Different_Weavings()
        {
            // -------------

            Q += Pluck_UserData();
            Q += Loom_UserData_To_UserWelcomingText();

            bool receivedOne = false;
            bool receivedTwo = false;

            Q += Weave_UserWelcomingText(onNext: u => receivedOne = true);
            Q += Weave_UserWelcomingText(onNext: u => receivedTwo = true);

            yield return null;

            Assert.IsTrue(receivedOne && receivedTwo);

            // -------------
        }
    }
}