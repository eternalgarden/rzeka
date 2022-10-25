using NUnit.Framework;
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public class Integration_Blockades_Forgotten_Scrolls : IntegrationBase
    {
        /*
         * TODO SO THIS IS ACTUALLY A GOOD IDEA TO USE ERIS EVENTS FOR ALL INTEGRATION TESTING IN RZEKA
         * This makes them less connected to the implementation of The Library and Scrolls themselves.
         * They are also more descriptive this way.
         * 
         * Other types of tests should be handled with unit tests that I am too lazy to write.
         *
         * TODO A variant on c with a looming dependency
         * TODO Looms and alterations with more than one dependency
         */

        [UnityTest]
        public IEnumerator a_IsAnActive_Alteration_ProperlyBlockedOnLost_Conjuring_Dependency()
        {
            // -------------

            bool wasProperlyBlocked = false;

            Q += Rzeka.Eris.RealmEventStream.Subscribe(e =>
            {
                if (e is not ScrollEvent { Scroll: AlteringScroll<UserData> } scrollEvent) return;

                if (scrollEvent.EventType.HasFlag(ScrollEventType.Blocked)) wasProperlyBlocked = true;
            });

            var singleDependency = Pluck_UserDataInterval();

            Q += Weave_UserData();

            singleDependency.Dispose();

            yield return null;

            Assert.IsTrue(wasProperlyBlocked);

            // -------------
        }
        
        [UnityTest]
        public IEnumerator b_IsAnActive_Alteration_ProperlyBlockedOnLost_Looming_Dependency()
        {
            // -------------

            // * straight from conjuring to weaving

            bool wasProperlyBlocked = false;

            Q += Rzeka.Eris.RealmEventStream.Subscribe(e =>
            {
                if (e is not ScrollEvent { Scroll: AlteringScroll<UserWelcomingText> } scrollEvent) return;

                if (scrollEvent.EventType.HasFlag(ScrollEventType.Blocked)) wasProperlyBlocked = true;
            });

            Q += Pluck_UserDataInterval();

            var dependency = Loom_UserData_To_UserWelcomingText();

            Q += Weave_UserWelcomingText();

            dependency.Dispose();

            yield return null;

            Assert.IsTrue(wasProperlyBlocked);

            // -------------
        }
        
        [UnityTest]
        public IEnumerator c_IsAnActive_Looming_ProperlyBlockedOnLost_Conjuring_Dependency()
        {
            // -------------

            bool wasProperlyBlocked = false;
            
            Q += Rzeka.Eris.RealmEventStream.Subscribe(e =>
            {
                if (e is not ScrollEvent { Scroll: LoomingScroll<UserData, UserWelcomingText> } scrollEvent) return;

                if (scrollEvent.EventType.HasFlag(ScrollEventType.Blocked)) wasProperlyBlocked = true;
            });

            var dependency = Pluck_UserDataInterval();

            Q += Loom_UserData_To_UserWelcomingText();

            dependency.Dispose();

            yield return null;

            Assert.IsTrue(wasProperlyBlocked);

            // -------------
        }
    }
}