/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
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
    public class Looms_Multiple_Ingredients : IntegrationBase
    {
        // -------------
        
        // [UnitySetUp]
        // public override IEnumerator Setup()
        // {
        //     // -------------

        //     yield return null;

        //     // -------------
        // }

        // [UnityTearDown]
        // public override IEnumerator Teardown()
        // {
        //     // -------------

        //     yield return null;

        //     // -------------
        // }
    
        [UnityTest]
        public IEnumerator a_()
        {
            // -------------
            
            int receivedWelcomingText = 0;

            using var x1 = Pluck_ANumber(1);
            using var x2 = Pluck_UserData(1);

            using var oik1 = Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Received && matterEvent.Matter.Type == typeof(UserWelcomingText))
                    receivedWelcomingText++;
            });

            using var oik2 = Rzeka.Loom<ANumber,UserData,UserWelcomingText>(
                this,
                obs => obs
                    .Select(_ => new UserWelcomingText("oik")) // * not important in this test
            );

            using var oik3 = Weave_UserWelcomingText(); // * necessary to close the chain

            yield return null;

            // ! 2 !!!!!!!!!!!
            AssertEqual(1, receivedWelcomingText);

            // -------------
        }

        [UnityTest]
        public IEnumerator a1_()
        {
            // -------------
            
            int receivedWelcomingText = 0;

            using var x1 = Pluck_ANumber(1);
            using var x2 = Pluck_AName("Alicja");

            using var oik1 = Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Received && matterEvent.Matter.Type == typeof(UserWelcomingText))
                    receivedWelcomingText++;
            });

            using var oik2 = Rzeka.Loom<ANumber,AName,UserWelcomingText>(
                this,
                obs => obs
                    .Select(_ => new UserWelcomingText("oik")) // * not important in this test
            );

            using var oik3 = Weave_UserWelcomingText(); // * necessary to close the chain

            yield return null;

            // ! 2 !!!!!!!!!!!
            AssertEqual(1, receivedWelcomingText);

            // -------------
        }

        [UnityTest]
        public IEnumerator a11_()
        {
            // -------------
            
            int receivedWelcomingText = 0;

            using var x1 = Pluck_ANumber(1);
            using var x2 = Pluck_AName("Alicja");

            using var oik2 = Rzeka.Loom<ANumber,AName,UserWelcomingText>(
                this,
                obs => obs
                    .Select(_ => new UserWelcomingText("oik")) // * not important in this test
            );

            using var oik3 = Weave_UserWelcomingText(
                onNext: _ => receivedWelcomingText++
            ); // * necessary to close the chain

            yield return new WaitForSeconds(0.1f);

            // ! 2 !!!!!!!!!!!
            AssertEqual(2, receivedWelcomingText);

            // -------------
        }
    
        [UnityTest]
        public IEnumerator b_()
        {
            // -------------

            int receivedUserData = 0;
            int receivedANumber = 0;

            using var oik1 = Rzeka.Loom<ANumber,UserData,UserWelcomingText>(
                this,
                obs => obs
                    .Do(glyph => {
                        if (glyph.One.Number == 1) receivedANumber++;
                        if (glyph.Two.Name == "Ali") receivedANumber++;
                    })
                    .Select(_ => new UserWelcomingText("oik")) // * not important in this test
            );

            using var oik2 = Weave_UserWelcomingText(); // * necessary to close the chain

            yield return null;

            // Because no observers
            AssertEqual((1,1), (receivedANumber, receivedUserData));

            // -------------
        }
    
        [UnityTest]
        public IEnumerator c_()
        {
            // -------------

            int receivedUserData = 0;
            int receivedANumber = 0;

            using var oik = Rzeka.Eris.RealmEventStream.Subscribe(onNext: next =>
            {
                if (next is not MatterEvent matterEvent) return;

                if (matterEvent.EventType == MatterEventType.Received && matterEvent.Matter.Type == typeof(UserData))
                    receivedUserData++;

                if (matterEvent.EventType == MatterEventType.Received && matterEvent.Matter.Type == typeof(ANumber))
                    receivedANumber++;
            });

            using var oik1 = Rzeka.Loom<ANumber,UserData,UserWelcomingText>(
                this,
                obs => obs
                    .Select(_ => new UserWelcomingText("oik")) // * not important in this test
            );

            using var oik2 = Weave_UserWelcomingText(); // * necessary to close the chain

            yield return null;

            // Because no observers
            AssertEqual((1,1), (receivedANumber, receivedUserData));

            // -------------
        }
        
        [UnityTest]
        public IEnumerator d_()
        {
            // -------------
            
            // using IRzeka Rzeka = new RzekaXOXO();

            int number = 0;
            string name = "";

            using var x1 = Pluck_ANumber(1);
            using var x2 = Pluck_AName("Alicja");

            using var oik1 = Rzeka.Loom<ANumber,AName,ANumberAndName>(
                this,
                obs => obs
                    .Select(glyph => new ANumberAndName(glyph.One.Number, glyph.Two.Name)) // * not important in this test
            );

            using var oik2 = Rzeka.Weave<ANumberAndName>(
                this,
                Observer.Create<ANumberAndName>(next => {
                    number = next.Number;
                    name = next.Name;
                })
            ); // * necessary to close the chain

            yield return null;

            // Because no observers
            AssertEqual((1,"Alicja"), (number, name));

            // -------------
        }
    
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 05 November 2022 🌊 */