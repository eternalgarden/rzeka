using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public class Integration_Eris_Spam_Verification : IntegrationBase
    {
        [UnityTest]
        public IEnumerator a_IsNewBlocked_Loom_LoggedOnlyOnce()
        {
            // -------------

            int count = 0;

            Q += Rzeka.Eris.RealmEventStream.Subscribe(e =>
            {
                if (e is not ScrollEvent { Scroll: LoomingScroll<UserData,UserWelcomingText> } scrollEvent) return;

                if (scrollEvent.EventType.HasFlag(ScrollEventType.Blocked)) count++;
            });

            Q += Loom_UserData_To_UserWelcomingText();

            yield return null;

            Assert.AreEqual(1,count);

            // -------------
        }
        
        [UnityTest]
        public IEnumerator b_IsNewBlocked_Weave_LoggedOnlyOnce()
        {
            // -------------

            int count = 0;

            Q += Weave_UserData();

            Q += Rzeka.Eris.RealmEventStream.Subscribe(e =>
            {
                if (e is not ScrollEvent { Scroll: AlteringScroll<UserData> } scrollEvent) return;

                if (scrollEvent.EventType.HasFlag(ScrollEventType.Blocked)) count++;
            });

            yield return null;

            Assert.AreEqual(1,count);

            // -------------
        }
        
        [UnityTest]
        public IEnumerator c_IsNewCast_Conjure_LoggedOnlyOnce()
        {
            // -------------

            int count = 0;

            Q += Rzeka.Eris.RealmEventStream.Subscribe(e =>
            {
                if (e is not ScrollEvent { Scroll: ConjuringScroll<UserData> } scrollEvent) return;

                if (scrollEvent.EventType.HasFlag(ScrollEventType.Cast)) count++;
            });

            Q += Pluck_UserDataInterval();

            yield return null;

            Assert.AreEqual(1,count);

            // -------------
        }
        
        [UnityTest]
        public IEnumerator c_IsNewCast_Loom_LoggedOnlyOnce()
        {
            // -------------

            int count = 0;

            Q += Rzeka.Eris.RealmEventStream.Subscribe(e =>
            {
                if (e is not ScrollEvent { Scroll: LoomingScroll<UserData,UserWelcomingText> } scrollEvent) return;

                if (scrollEvent.EventType.HasFlag(ScrollEventType.Cast)) count++;
            });

            Q += Pluck_UserDataInterval();
            Q += Loom_UserData_To_UserWelcomingText();

            yield return null;

            Assert.AreEqual(1,count);

            // -------------
        }
        
        [UnityTest]
        public IEnumerator c_IsNewCast_Weave_LoggedOnlyOnce()
        {
            // -------------

            int count = 0;

            Q += Rzeka.Eris.RealmEventStream.Subscribe(e =>
            {
                if (e is not ScrollEvent { Scroll: AlteringScroll<UserWelcomingText> } scrollEvent) return;

                if (scrollEvent.EventType.HasFlag(ScrollEventType.Cast)) count++;
            });

            Q += Pluck_UserDataInterval();
            Q += Loom_UserData_To_UserWelcomingText();
            Q += Weave_UserWelcomingText();

            yield return null;

            Assert.AreEqual(1,count);

            // -------------
        }
    }
}