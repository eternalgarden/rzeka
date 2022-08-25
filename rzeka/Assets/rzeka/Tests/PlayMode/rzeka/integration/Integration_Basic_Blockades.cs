
using NUnit.Framework;
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public class Integration_Basic_Blockades
    {
        RzekaXOXO Rzeka;
        CollectibleDisposable _disposables;
        IDisposable _userDataMatter;
        IDisposable _userWelcomingTextMatter;
        IDisposable _userWelcomingTextWeaver;

        [UnitySetUp]
        public IEnumerator Setup()
        {
            // -------------

            Rzeka = new RzekaXOXO();
            _disposables = new();

            yield return null;

            // -------------
        }

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

        [UnityTest]
        public IEnumerator a_Is_AlteringScroll_properly_saved_as_blocked()
        {
            // -------------

            WeaveWithUserWelcomingText();

            yield return null;

            if (Rzeka.TheLibrary.IsTypeBlockingSpells<UserWelcomingText>(out TBindingScroll[] blockedScrolls))
            {
                Assert.IsTrue(blockedScrolls.Length == 1);
            }
            else
            {
                Assert.Fail("No blocked scrolls found");
            }

            // -------------
        }

        [UnityTest]
        public IEnumerator a_Is_BindingScroll_properly_saved_as_blocked()
        {
            // -------------

            LoomUserWelcomingText();

            yield return null;

            if (Rzeka.TheLibrary.IsTypeBlockingSpells<UserData>(out TBindingScroll[] blockedScrolls))
            {
                Assert.IsTrue(blockedScrolls.Length == 1);
            }
            else
            {
                Assert.Fail("No blocked scrolls found");
            }

            // -------------
        }

        [UnityTest]
        public IEnumerator b_Is_codependent_altering_binding_block_properly_registered()
        {
            // -------------

            WeaveWithUserWelcomingText();
            LoomUserWelcomingText();

            yield return null;

            bool oneUserWelcomingBlocked = false;
            bool oneUserDataBlocked = false;

            if (Rzeka.TheLibrary.IsTypeBlockingSpells<UserData>(out TBindingScroll[] blockedScrolls))
            {
                oneUserDataBlocked = blockedScrolls.Length == 1;
            }
            else
            {
                Assert.Fail("No scrolls blocked by UserData type found");
            }

            if (Rzeka.TheLibrary.IsTypeBlockingSpells<UserWelcomingText>(out TBindingScroll[] userDataBlocked))
            {
                oneUserWelcomingBlocked = userDataBlocked.Length == 1;
            }
            else
            {
                Assert.Fail("No scrolls blocked by UserWelcomingText type found");
            }

            Assert.True(oneUserDataBlocked && oneUserWelcomingBlocked);

            // -------------
        }

        [UnityTest]
        public IEnumerator c_Is_conjurable_of_UserData_ReversedOrder()
        {
            // -------------

            WeaveWithUserWelcomingText();
            LoomUserWelcomingText();
            PluckUserDataMatter();

            Assert.IsTrue(Rzeka.TheLibrary.IsConjurable<UserData>(out _));

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator c_Is_conjurable_of_UserWelcomingText_ReversedOrder()
        {
            // -------------

            WeaveWithUserWelcomingText();
            LoomUserWelcomingText();
            PluckUserDataMatter();

            Assert.IsTrue(Rzeka.TheLibrary.IsConjurable<UserWelcomingText>(out _));

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator c_Is_Weave_UserWelcomingText_Received_ReversedOrder()
        {
            // -------------

            WeaveWithUserWelcomingText();
            LoomUserWelcomingText();
            PluckUserDataMatter();

            bool received = false;

            using var _ = Rzeka.Weave<UserWelcomingText>(
                who: this,
                spell: Observer.Create<UserWelcomingText>(onNext: u => received = true));

            yield return null;

            Assert.IsTrue(received);

            // -------------
        }

        [UnityTest]
        public IEnumerator z_Is_unblocked_BindingScroll_properly_removed_from_blocked_scrolls_collection()
        {
            // -------------

            WeaveWithUserWelcomingText();
            LoomUserWelcomingText();
            PluckUserDataMatter();

            yield return null;

            if (Rzeka.TheLibrary.IsTypeBlockingSpells<UserData>(out TBindingScroll[] blockedByUserData))
            {
                Assert.Fail("scrolls blocked by UserData type found, array length: {0}", blockedByUserData.Length);
            }

            Assert.True(true);

            // -------------
        }

        [UnityTest]
        public IEnumerator z_Is_unblocked_AlteringScroll_properly_removed_from_blocked_scrolls_collection()
        {
            // -------------

            WeaveWithUserWelcomingText();
            LoomUserWelcomingText();
            PluckUserDataMatter();

            yield return null;

            if (Rzeka.TheLibrary.IsTypeBlockingSpells<UserWelcomingText>(out TBindingScroll[] blockedByWelcoming))
            {
                Assert.Fail("scrolls blocked by UserWelcomingText type found, array length: {0}", blockedByWelcoming.Length);
            }

            Assert.True(true);

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