using System.Collections;
using UnityEngine.TestTools;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rzeka.Tests.Integration
{
    public class Integration_Eris_Matter : IntegrationBase
    {
        [UnityTest]
        public IEnumerator a_sd()
        {
            // -------------

            const int EXPECTED_COUNT = 1;

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;
                
                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData)) shapedMatterEventCount++;
            });

            Pluck_UserData(EXPECTED_COUNT);
            Weave_UserData();

            yield return null;
            
            AssertEqual(EXPECTED_COUNT, shapedMatterEventCount);

            // -------------
        }
        
        [UnityTest]
        public IEnumerator b0_PluckWithNoObservers_Cold()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;
                
                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData)) shapedMatterEventCount++;
            });
            
            Pluck_UserData(1);

            yield return null;
            
            // Because no observers
            AssertEqual(0, shapedMatterEventCount);

            // -------------
        }
        
        [UnityTest]
        public IEnumerator b1_()
        {
            // -------------

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;
                
                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData)) shapedMatterEventCount++;
            });

            Pluck_UserData(1);
            Loom_UserData_To_UserWelcomingText();

            yield return null;
            
            // because no observers, there is no weaving
            AssertEqual(0, shapedMatterEventCount);

            // -------------
        }
        
        
        [UnityTest]
        public IEnumerator b2_()
        {
            // -------------

            const int EXPECTED_COUNT = 1;

            int shapedMatterEventCount = 0;

            Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;
                
                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData)) shapedMatterEventCount++;
            });

            Pluck_UserData(EXPECTED_COUNT);
            Loom_UserData_To_UserWelcomingText();
            Weave_UserWelcomingText();

            yield return null;
            
            AssertEqual(EXPECTED_COUNT, shapedMatterEventCount);

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
                
                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData)) shapedMatterEventCount++;
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
                
                if (matterEvent.EventType == MatterEventType.Shaped && matterEvent.Matter.Type == typeof(UserData)) shapedMatterEventCount++;
            });

            IConnectableObservable<UserData> observable = Observable
                .Timer(TimeSpan.FromSeconds(0.1))
                .Select(_ => new UserData("xx", "uu", 555))
                .Publish();
            
            Q += Rzeka.Pluck<UserData>(this, spell: observable);
            Q += observable.Connect();

            yield return new WaitForSeconds(0.2f);

            Debug.Log(shapedMatterEventCount);
            
            // ! even though no observers expected one
            AssertEqual(1, shapedMatterEventCount);

            // -------------
        }
    }
}
