using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public abstract class IntegrationBase
    {
        protected RzekaXOXO Rzeka;
        protected CollectibleDisposable Q;

        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            Rzeka = new RzekaXOXO();
            Q = new CollectibleDisposable();

            yield return null;

            // -------------
        }

        [UnityTearDown]
        public IEnumerator Teardown()
        {
            // -------------

            Q?.Dispose();
            Rzeka.Dispose();

            yield return null;

            // -------------
        }

        protected IDisposable Pluck_UserDataInterval()
        {
            return Rzeka.Pluck<UserData>(
                who: this,
                spell: Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Select(x => new UserData
                    {
                        Name = "Maria", 
                        Zodiac = "Cancer", 
                        FavNumber = (int)x, 
                        JoinedDate = new DateTime(1992, 7, 3)
                    }));
        }

        protected IDisposable Loom_UserData_To_UserWelcomingText()
        {
            return Rzeka.Loom<UserData, UserWelcomingText>(
                who: this,
                spell: userData => userData
                    .Select(dd => new UserWelcomingText
                    {
                        WelcomingText = $"Hi Maria! Ur fav number is <<{dd.FavNumber}>> a {dd.Zodiac} who joined us {(int)(DateTime.Now - dd.JoinedDate).TotalDays} days ago."
                    }));
        }
        
        protected IDisposable Weave_UserData(Action<UserData> onNext = null)
        {
            onNext ??= _ => { };

            return Rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext));
        }
        
        protected IDisposable Weave_UserWelcomingText(Action<UserWelcomingText> onNext = null)
        {
            onNext ??= _ => { };
            
            return Rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext));
        }
    }
}
