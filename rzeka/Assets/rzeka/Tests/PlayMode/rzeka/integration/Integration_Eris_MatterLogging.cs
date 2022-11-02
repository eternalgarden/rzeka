using System.Collections;
using UnityEngine.TestTools;
using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rzeka.Tests.Integration
{
    public class Integration_Eris_MatterLogging : IntegrationBase
    {
        [UnityTest]
        public IEnumerator a0_Expected_0_MatterShaped_Pluc_NoObserver_Cold()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedMatterEventCount++;
            });

            Q += Pluck_UserData(1);

            yield return null;

            // Because no observers
            AssertEqual(0, shapedMatterEventCount);

            // -------------
        }

        [UnityTest]
        public IEnumerator a1_Expected_0_MtterShaped_PluckWithALoom_NoObserver_Cold()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedMatterEventCount++;
            });

            Q += Pluck_UserData(1);
            Q += Loom_UserData_To_UserWelcomingText();

            yield return null;

            // because no observers, there is no weaving
            AssertEqual(0, shapedMatterEventCount);

            // -------------
        }

        [UnityTest]
        public IEnumerator b1_Expected_1_MatterShaped_PlucDirectWeave_Cold()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedMatterEventCount++;
            });

            Q += Pluck_UserData(1);
            Q += Weave_UserData();

            yield return null;

            AssertEqual(1, shapedMatterEventCount);

            // -------------
        }

        [UnityTest]
        public IEnumerator b2_Expected_1_MatterShaped_PlucLoomWeave_Cold()
        {
            // -------------

            int shapedUserDataMatter = 0;
            int shapedWelcomingTextMatter = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedUserDataMatter++;
                if (matterEvent.EventType == MatterEventType.Shaped &&
                    matterEvent.Matter.Type == typeof(UserWelcomingText)) shapedWelcomingTextMatter++;
            });

            Q += Pluck_UserData(1);
            Q += Loom_UserData_To_UserWelcomingText();
            Q += Weave_UserWelcomingText();

            yield return null;

            AssertEqual((1, 1), (shapedUserDataMatter, shapedWelcomingTextMatter));

            // -------------
        }

        [UnityTest]
        public IEnumerator d_PluckWithNoObservers_Hot_RefCount()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedMatterEventCount++;
            });

            Q += Rzeka.Pluck<UserData>(this, spell: Observable
                .Timer(TimeSpan.FromSeconds(0.1))
                .Select(_ => new UserData("xx", "uu", 555))
                .Publish()
                .RefCount());

            yield return new WaitForSeconds(0.2f);

            // Because no observers
            AssertEqual(0, shapedMatterEventCount);

            // -------------
        }

        [UnityTest]
        public IEnumerator e_PluckWithNoObservers_Hot_ConnectedPublish()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedMatterEventCount++;
            });

            IConnectableObservable<UserData> observable = Observable
                .Timer(TimeSpan.FromSeconds(0.1))
                .Select(_ => new UserData("xx", "uu", 555))
                .Publish();

            Q += Rzeka.Pluck<UserData>(this, spell: observable);
            Q += observable.Connect();

            yield return new WaitForSeconds(0.2f);

            // Also 0 because Do will not be triggered without observers
            // TODO if it bothers you, ie. logs of
            AssertEqual(0, shapedMatterEventCount);

            // -------------
        }

        [UnityTest]
        public IEnumerator e1_PluckWeave_Hot_ConnectedPublish()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedMatterEventCount++;
            });

            IConnectableObservable<UserData> observable = Observable
                .Timer(TimeSpan.FromSeconds(0.1))
                .Select(_ => new UserData("xx", "uu", 555))
                .Publish();

            Q += Rzeka.Pluck<UserData>(this, spell: observable);
            Q += observable.Connect();

            yield return new WaitForSeconds(0.2f);

            Q += Weave_UserData();

            AssertEqual(0, shapedMatterEventCount);

            // -------------
        }

        [UnityTest]
        public IEnumerator e1x_PluckWeave_Hot_Replay1()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedMatterEventCount++;
            });

            IConnectableObservable<UserData> observable = Observable
                .Timer(TimeSpan.FromSeconds(0.1))
                .Select(_ => new UserData("xx", "uu", 555))
                .Replay(1);

            Q += Rzeka.Pluck<UserData>(this, spell: observable);
            Q += observable.Connect();

            yield return new WaitForSeconds(0.2f);

            Q += Weave_UserData();

            AssertEqual(1, shapedMatterEventCount);

            // -------------
        }

        [UnityTest]
        public IEnumerator e2_PluckWeave_Hot_ConnectedPublish()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedMatterEventCount++;
            });

            IConnectableObservable<UserData> observable = Observable
                .Timer(TimeSpan.FromSeconds(0.1))
                .Select(_ => new UserData("xx", "uu", 555))
                .Publish();

            Q += Rzeka.Pluck<UserData>(this, spell: observable);
            Q += observable.Connect();
            Q += Weave_UserData();

            yield return new WaitForSeconds(0.2f);

            AssertEqual(1, shapedMatterEventCount);

            // -------------
        }

        [UnityTest]
        public IEnumerator f1a_Hot_Replay_Pluck_2Weave_Yield_BeforeSub()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedMatterEventCount++;
            });

            IConnectableObservable<UserData> observable = Observable
                .Timer(TimeSpan.FromSeconds(0.1))
                .Select(_ => new UserData("xx", "uu", 555))
                .Replay(1);

            Q += Rzeka.Pluck<UserData>(this, spell: observable);
            Q += observable.Connect();

            yield return new WaitForSeconds(0.2f);

            Q += Weave_UserData();
            Q += Weave_UserData();

            AssertEqual(1, shapedMatterEventCount);

            // -------------
        }
        
        [UnityTest]
        public IEnumerator f1b_Hot_Replay_Pluck_2Weave_Yield_AfterSub()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedMatterEventCount++;
            });

            IConnectableObservable<UserData> observable = Observable
                .Timer(TimeSpan.FromSeconds(0.1))
                .Select(_ => new UserData("xx", "uu", 555))
                .Replay(1);

            Q += Rzeka.Pluck<UserData>(this, spell: observable);
            Q += observable.Connect();

            Q += Weave_UserData();
            Q += Weave_UserData();
            
            yield return new WaitForSeconds(0.2f);

            AssertEqual(2, shapedMatterEventCount);

            // -------------
        }
        
        [UnityTest]
        public IEnumerator f3a_Hot_Publish_Pluck_2Weave_Yield_BeforeSub()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedMatterEventCount++;
            });

            IConnectableObservable<UserData> observable = Observable
                .Timer(TimeSpan.FromSeconds(0.1))
                .Select(_ => new UserData("xx", "uu", 555))
                .Publish();

            Q += Rzeka.Pluck<UserData>(this, spell: observable);
            Q += observable.Connect();

            yield return new WaitForSeconds(0.2f);
            
            Q += Weave_UserData();
            Q += Weave_UserData();

            AssertEqual(0, shapedMatterEventCount);

            // -------------
        }

        [UnityTest]
        public IEnumerator f3b_Hot_Publish_Pluck_2Weave_Yield_AfterSub()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData))
                    shapedMatterEventCount++;
            });

            IConnectableObservable<UserData> observable = Observable
                .Timer(TimeSpan.FromSeconds(0.1))
                .Select(_ => new UserData("xx", "uu", 555))
                .Publish();

            Q += Rzeka.Pluck<UserData>(this, spell: observable);
            Q += observable.Connect();

            Q += Weave_UserData();
            Q += Weave_UserData();
            
            yield return new WaitForSeconds(0.2f);

            AssertEqual(2, shapedMatterEventCount);

            // -------------
        }

        [UnityTest]
        public IEnumerator g_SanityCheck_Process_Cold()
        {

            int countCnonjuring = 0;
            int countLoom = 0;
            
            const int count = 1;
            
            var userDataObservable = Observable
                .Create<UserData>(observer =>
                {
                    for (int i = 0; i < count; i++)
                    {
                        observer.OnNext(new UserData("Ali", "Roofwalking Cat", i));
                    }
                        
                    observer.OnCompleted();
                        
                    return Disposable.Empty;
                })
                .Do(next => countCnonjuring++);

            Func<IObservable<UserData>, IObservable<UserWelcomingText>> loomSpell = observable => observable
                .Select(dd =>
                    new UserWelcomingText($"Hi {dd.Name}! Ur fav number is <<{dd.FavNumber}>>, a {dd.Zodiac}"))
                .Do(next => countLoom++);

            var loomObservable = loomSpell.Invoke(userDataObservable);

            var observerUserData = Observer.Create<UserData>(
                onNext: _ => { });
            
            var observerWelcomingText = Observer.Create<UserWelcomingText>(
                onNext: _ => { });

            Q += userDataObservable.Subscribe(observerUserData);
            Q += userDataObservable.Subscribe(observerUserData);
            Q += loomObservable.Subscribe(observerWelcomingText);
            
            yield return null;
            
            AssertEqual((3,1), (countCnonjuring,countLoom));
        }
        
        [UnityTest]
        public IEnumerator g1_SanityCheck_Process_Hot_PrePublishLogging()
        {
            int countCnonjuring = 0;
            int countLoom = 0;
            int receivedUserData = 0;
            
            const int count = 1;
            
            var userDataObservable = Observable
                .Interval(TimeSpan.FromSeconds(0.1f))
                .Select(x => new UserData("Ali", "Roofwalking Cat", (int)x))
                .Take(3)
                .Do(next => countCnonjuring++)
                .Publish();

            Q += userDataObservable.Connect();
            
            yield return new WaitForSeconds(0.15f); // one gets lost
            
            Func<IObservable<UserData>, IObservable<UserWelcomingText>> loomSpell = observable => observable
                .Select(dd =>
                    new UserWelcomingText($"Hi {dd.Name}! Ur fav number is <<{dd.FavNumber}>>, a {dd.Zodiac}"))
                .Do(next =>
                {
                    countLoom++;
                    receivedUserData++;
                });

            var loomObservable = loomSpell.Invoke(userDataObservable);

            var observerUserData = Observer.Create<UserData>(
                onNext: _ => receivedUserData++);
            
            var observerWelcomingText = Observer.Create<UserWelcomingText>(
                onNext: _ => { });

            Q += userDataObservable.Subscribe(observerUserData);
            // Q += userDataObservable.Subscribe(observerUserData);
            Q += loomObservable.Subscribe(observerWelcomingText);
            
            yield return new WaitForSeconds(1f);
            
            AssertEqual((3,2,4), (countCnonjuring,countLoom,receivedUserData));
        }
        
        [UnityTest]
        public IEnumerator g2_SanityCheck_Process_Hot_PostPublishLogging()
        {
            int conjuredCount = 0;
            int countLoom = 0;
            int receivedUserData = 0;
            
            const int count = 1;
            
            var source = Observable
                .Interval(TimeSpan.FromSeconds(0.1f))
                .Select(x => new UserData("Ali", "Roofwalking Cat", (int)x))
                .Take(2)
                .Publish();

            Q += source.Connect();
            
            yield return new WaitForSeconds(0.15f); // one gets lost
            
            var userDataObservable = source
                .Do(next => conjuredCount++);

            Func<IObservable<UserData>, IObservable<UserWelcomingText>> loomSpell = observable => observable
                .Select(dd =>
                    new UserWelcomingText($"Hi {dd.Name}! Ur fav number is <<{dd.FavNumber}>>, a {dd.Zodiac}"))
                .Do(next =>
                {
                    countLoom++;
                    receivedUserData++;
                });

            var loomObservable = loomSpell.Invoke(userDataObservable);

            var observerUserData = Observer.Create<UserData>(
                onNext: _ => receivedUserData++);
            
            var observerWelcomingText = Observer.Create<UserWelcomingText>(
                onNext: _ => { });

            Q += userDataObservable.Subscribe(observerUserData);
            // Q += userDataObservable.Subscribe(observerUserData);
            Q += loomObservable.Subscribe(observerWelcomingText);
            
            yield return new WaitForSeconds(1f);
            
            AssertEqual((conjuredCount: 2, countLoom: 1, receivedUserData: 2), (conjuredCount,countLoom,receivedUserData));
        }
        
        
        [UnityTest]
        public IEnumerator g3_SanityCheck_Process_Hot_JustReceivalsLogging()
        {
            int countLoom = 0;
            int receivedUserData = 0;
            
            const int count = 1;
            
            var userDataObservable = Observable
                .Interval(TimeSpan.FromSeconds(0.1f))
                .Select(x => new UserData("Ali", "Roofwalking Cat", (int)x))
                .Take(2)
                .Publish();

            Q += userDataObservable.Connect();

            yield return new WaitForSeconds(0.15f); // one gets lost

            Func<IObservable<UserData>, IObservable<UserWelcomingText>> loomSpell = observable => observable
                .Select(dd =>
                    new UserWelcomingText($"Hi {dd.Name}! Ur fav number is <<{dd.FavNumber}>>, a {dd.Zodiac}"))
                .Do(next =>
                {
                    countLoom++;
                    receivedUserData++;
                });

            var loomObservable = loomSpell.Invoke(userDataObservable);

            var observerUserData = Observer.Create<UserData>(
                onNext: _ => receivedUserData++);
            
            var observerWelcomingText = Observer.Create<UserWelcomingText>(
                onNext: _ => { });

            Q += userDataObservable.Subscribe(observerUserData);
            // Q += userDataObservable.Subscribe(observerUserData);
            Q += loomObservable.Subscribe(observerWelcomingText);
            
            yield return new WaitForSeconds(1f);
            
            AssertEqual((1,2), (countLoom,receivedUserData));
        }
    }
}