using NUnit.Framework;
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public class Integration_Blockades_Forgotten_Scrolls
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

            var singleDependency = Rzeka.Pluck<UserData>(
                who: this,
                spell: Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Select(_ => new UserData
                        { Name = "Maria", Zodiac = "Cancer", FavNumber = 7, JoinedDate = new DateTime(1992, 7, 3) }));

            _disposables += Rzeka.Weave<UserData>(
                who: this,
                spell: Observer.Create<UserData>(
                    onNext: _ => { }));

            _disposables += Rzeka.Eris.RealmEventStream.Subscribe(e =>
            {
                if (e is not ScrollEvent { Scroll: AlteringScroll<UserData> } scrollEvent) return;

                if (scrollEvent.EventType.HasFlag(ScrollEventType.Blocked)) wasProperlyBlocked = true;
            });

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

            _disposables += Rzeka.Pluck<UserData>(
                who: this,
                spell: Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Select(_ => new UserData
                        { Name = "Maria", Zodiac = "Cancer", FavNumber = 7, JoinedDate = new DateTime(1992, 7, 3) }));
            
            var dependency = Rzeka.Loom<UserData, UserWelcomingText>(
                who: this,
                spell: userData => userData
                    .Select(_ => new UserWelcomingText()));
            
            _disposables += Rzeka.Weave<UserWelcomingText>(
                who: this,
                spell: Observer.Create<UserWelcomingText>(
                    onNext: _ => { }));

            _disposables += Rzeka.Eris.RealmEventStream.Subscribe(e =>
            {
                if (e is not ScrollEvent { Scroll: AlteringScroll<UserWelcomingText> } scrollEvent) return;

                if (scrollEvent.EventType.HasFlag(ScrollEventType.Blocked)) wasProperlyBlocked = true;
            });

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

            var dependency = Rzeka.Pluck<UserData>(
                who: this,
                spell: Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Select(_ => new UserData
                        { Name = "Maria", Zodiac = "Cancer", FavNumber = 7, JoinedDate = new DateTime(1992, 7, 3) }));
            
            _disposables += Rzeka.Loom<UserData, UserWelcomingText>(
                who: this,
                spell: userData => userData
                    .Select(_ => new UserWelcomingText()));
            
            _disposables += Rzeka.Eris.RealmEventStream.Subscribe(e =>
            {
                if (e is not ScrollEvent { Scroll: LoomingScroll<UserData, UserWelcomingText> } scrollEvent) return;

                if (scrollEvent.EventType.HasFlag(ScrollEventType.Blocked)) wasProperlyBlocked = true;
            });

            dependency.Dispose();

            yield return null;

            Assert.IsTrue(wasProperlyBlocked);

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