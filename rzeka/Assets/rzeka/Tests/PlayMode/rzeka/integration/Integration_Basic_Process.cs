
using NUnit.Framework;
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public class Integration_Basic_Process
    {
        RzekaXOXO Rzeka;
        CollectibleDisposable _disposables;
        IDisposable _userDataMatter;
        IDisposable _userWelcomingTextMatter;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // -------------

            Rzeka = new RzekaXOXO();
            _disposables = new();

            _userDataMatter = Rzeka.Pluck<UserData>(
                who: this, 
                spell: Observable
                    .Return(new UserData { Name = "Maria", Zodiac = "Cancer", FavNumber = 7, JoinedDate = new DateTime(1992, 7, 3) }));

            _userWelcomingTextMatter = Rzeka.Loom<UserData, UserWelcomingText>(
                who: this,
                spell: userData => userData
                    .Select(dd => new UserWelcomingText { WelcomingText = $"Hi Maria! Ur a {dd.Zodiac} who joined us {(int)(DateTime.Now - dd.JoinedDate).TotalDays} days ago." }));

            yield return null;
            
            // -------------
        }

        [UnityTest]
        public IEnumerator a_Is_conjurable_of_UserData()
        {
            // -------------

            Assert.IsTrue(Rzeka.Library.IsConjurable<UserData>(out _));

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a_Is_conjurable_of_UserWelcomingText()
        {
            // -------------

            Assert.IsTrue(Rzeka.Library.IsConjurable<UserWelcomingText>(out _));

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b_Is_Weave_UserWelcomingText_Received()
        {
            // -------------

            bool received = false;

            using var _ = Rzeka.Weave<UserWelcomingText>(
                who: this,
                spell: Observer.Create<UserWelcomingText>(onNext: u => received = true));

            yield return null;

            Assert.IsTrue(received);

            // -------------
        }

        [UnityTest]
        public IEnumerator b_Is_Weave_UserWelcomingText_Received_Twice()
        {
            // -------------

            bool receivedOne = false;
            bool receivedTwo = false;

            using var one = Rzeka.Weave<UserWelcomingText>(
                who: this,
                spell: Observer.Create<UserWelcomingText>(onNext: u => receivedOne = true));

            using var two = Rzeka.Weave<UserWelcomingText>(
                who: this,
                spell: Observer.Create<UserWelcomingText>(onNext: u => receivedTwo = true));

            yield return null;

            Assert.IsTrue(receivedOne && receivedTwo);

            // -------------
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            // -------------

            _disposables.Dispose();
            _userDataMatter.Dispose();
            _userWelcomingTextMatter.Dispose();

            yield return null;

            // -------------
        }
    }
}