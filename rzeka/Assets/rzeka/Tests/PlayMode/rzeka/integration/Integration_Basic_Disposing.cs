
using NUnit.Framework;
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public class Integration_Basic_Disposing : IntegrationBase
    {
        [UnityTest]
        public IEnumerator a_Is_a_conjurable_ConjuringScroll_properly_disposed()
        {
            // -------------

            IDisposable d = Pluck_UserData();
            d.Dispose();

            yield return null;

            Assert.IsFalse(Rzeka.TheLibrary.IsConjurable<UserData>());

            // -------------
        }

        [UnityTest]
        public IEnumerator b_Is_a_conjurable_BindingScroll_properly_disposed()
        {
            // -------------

            IDisposable d = Loom_UserData_To_UserWelcomingText();
            d.Dispose();

            yield return null;

            Assert.IsFalse(Rzeka.TheLibrary.IsConjurable<UserWelcomingText>() || Rzeka.TheLibrary.IsTypeBlockingSpells<UserData>(out _));

            // -------------
        }

        [UnityTest]
        public IEnumerator c_Is_a_blocked_BindingScroll_properly_disposed()
        {
            // -------------

            IDisposable d = Loom_UserData_To_UserWelcomingText();
            d.Dispose();

            yield return null;

            Assert.IsFalse(Rzeka.TheLibrary.IsTypeBlockingSpells<UserData>(out _));

            // -------------
        }

        [UnityTest]
        public IEnumerator d_Is_a_blocked_AlteringScroll_properly_disposed()
        {
            // -------------

            IDisposable d = Weave_UserWelcomingText();
            d.Dispose();

            yield return null;

            Assert.IsFalse(Rzeka.TheLibrary.IsTypeBlockingSpells<UserWelcomingText>(out _));

            // -------------
        }
    }
}