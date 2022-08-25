
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
        IDisposable _userWelcomingTextWeaver;

        void PluckUserDataMatter()
        {
            _userDataMatter = Rzeka.Pluck<UserData>(
                who: this,
                spell: Observable
                    .Return(new UserData { Name = "Maria", Zodiac = "Cancer", FavNumber = 7, JoinedDate = new DateTime(1992, 7, 3) }));
        }

        void LoomUserWelcomingText()
        {
            _userWelcomingTextMatter = Rzeka.Loom<UserData, UserWelcomingText>(
                who: this,
                spell: userData => userData
                    .Select(dd => new UserWelcomingText { WelcomingText = $"Hi Maria! Ur a {dd.Zodiac} who joined us {(int)(DateTime.Now - dd.JoinedDate).TotalDays} days ago." }));
        }

        void WeaveWithUserWelcomingText()
        {
            _userWelcomingTextWeaver = Rzeka.Weave<UserWelcomingText>(
                who: this,
                spell: Observer.Create<UserWelcomingText>(onNext: u => { }));
        }

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // -------------

            Rzeka = new RzekaXOXO();
            _disposables = new();

            yield return null;
            
            // -------------
        }

        [UnityTest]
        public IEnumerator a_Is_conjurable_of_UserData()
        {
            // -------------

            PluckUserDataMatter();
            Assert.IsTrue(Rzeka.TheLibrary.IsConjurable<UserData>(out _));

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator a_Is_conjurable_of_UserWelcomingText()
        {
            // -------------

            PluckUserDataMatter();
            LoomUserWelcomingText();

            Assert.IsTrue(Rzeka.TheLibrary.IsConjurable<UserWelcomingText>(out _));

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator b_Is_Weave_UserWelcomingText_Received()
        {
            // -------------

            PluckUserDataMatter();
            LoomUserWelcomingText();

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

            PluckUserDataMatter();
            LoomUserWelcomingText();

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

            _disposables?.Dispose();
            _userDataMatter?.Dispose();
            _userWelcomingTextMatter?.Dispose();
            _userWelcomingTextWeaver?.Dispose();
            Rzeka.Dispose();

            yield return null;

            // -------------
        }
    }
}