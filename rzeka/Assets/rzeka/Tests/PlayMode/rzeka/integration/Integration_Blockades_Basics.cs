
using NUnit.Framework;
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public class Integration_Blockades_Basics : IntegrationBase
    {
        [UnityTest]
        public IEnumerator a_Is_AlteringScroll_properly_saved_as_blocked()
        {
            // -------------

            Q += Weave_UserWelcomingText();

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

            Q += Loom_UserData_To_UserWelcomingText();

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
            
            Q += Weave_UserWelcomingText();
            Q += Loom_UserData_To_UserWelcomingText();


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
            
            Q += Weave_UserWelcomingText();
            Q += Loom_UserData_To_UserWelcomingText();
            Q += Pluck_UserData();

            Assert.IsTrue(Rzeka.TheLibrary.IsConjurable<UserData>());

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator c_Is_conjurable_of_UserWelcomingText_ReversedOrder()
        {
            // -------------
            
            Q += Weave_UserWelcomingText();
            Q += Loom_UserData_To_UserWelcomingText();
            Q += Pluck_UserData();

            Assert.IsTrue(Rzeka.TheLibrary.IsConjurable<UserWelcomingText>());

            yield return null;

            // -------------
        }

        [UnityTest]
        public IEnumerator c_Is_Weave_UserWelcomingText_Received_ReversedOrder()
        {
            // -------------
            
            Q += Weave_UserWelcomingText();
            Q += Loom_UserData_To_UserWelcomingText();
            Q += Pluck_UserData();

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
            
            Q += Weave_UserWelcomingText();
            Q += Loom_UserData_To_UserWelcomingText();
            Q += Pluck_UserData();

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
            
            Q += Weave_UserWelcomingText();
            Q += Loom_UserData_To_UserWelcomingText();
            Q += Pluck_UserData();

            yield return null;

            if (Rzeka.TheLibrary.IsTypeBlockingSpells<UserWelcomingText>(out TBindingScroll[] blockedByWelcoming))
            {
                Assert.Fail("scrolls blocked by UserWelcomingText type found, array length: {0}", blockedByWelcoming.Length);
            }

            Assert.True(true);

            // -------------
        }
    }
}