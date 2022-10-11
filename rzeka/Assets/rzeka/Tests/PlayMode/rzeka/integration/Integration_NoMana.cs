
using NUnit.Framework;
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public class Integration_NoMana
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
        public IEnumerator a_Is_a_conjurable_ConjuringScroll_properly_disposed()
        {
            // -------------

            PluckUserDataMatter();
            LoomUserWelcomingText();

            _userDataMatter.Dispose();

            yield return null;

            Assert.IsFalse(Rzeka.TheLibrary.IsConjurable<UserData>());

            // -------------
        }

        [UnityTest]
        public IEnumerator a_Is_a_conjurable_BindingScroll_properly_disposed()
        {
            // -------------

            PluckUserDataMatter();
            LoomUserWelcomingText();

            _userWelcomingTextMatter.Dispose();

            yield return null;

            Assert.IsFalse(Rzeka.TheLibrary.IsConjurable<UserWelcomingText>());

            // -------------
        }

        [UnityTest]
        public IEnumerator b_Is_a_blocked_AlteringScroll_properly_disposed()
        {
            // -------------

            WeaveWithUserWelcomingText();

            _userWelcomingTextWeaver.Dispose();

            yield return null;

            Assert.IsFalse(Rzeka.TheLibrary.IsTypeBlockingSpells<UserWelcomingText>(out _));

            // -------------
        }

        [UnityTest]
        public IEnumerator b_Is_a_blocked_BindingScroll_properly_disposed()
        {
            // -------------

            LoomUserWelcomingText();

            _userWelcomingTextMatter.Dispose();

            yield return null;

            Assert.IsFalse(Rzeka.TheLibrary.IsTypeBlockingSpells<UserData>(out _));

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