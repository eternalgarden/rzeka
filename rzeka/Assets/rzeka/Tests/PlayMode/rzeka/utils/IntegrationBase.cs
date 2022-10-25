using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Integration
{
    public abstract class IntegrationBase
    {
        protected RzekaXOXO Rzeka;
        protected CollectibleDisposable Q;

        protected static void AssertEqual<T>(T expected, T actual)
        {
            Assert.AreEqual(expected, actual, $"Expected: {expected} while actual: {actual}.");
        }

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
        
        protected IDisposable Pluck_UserData(int count = 1)
        {
            return Rzeka.Pluck<UserData>(
                who: this,
                spell: Observable
                    .Create<UserData>(observer =>
                    {
                        for (int i = 0; i < count; i++)
                        {
                            observer.OnNext(new UserData("Ali", "Roofwalking Cat", i));
                        }
                        
                        observer.OnCompleted();
                        
                        return Disposable.Empty;
                    }));
        }

        protected IDisposable Pluck_UserDataInterval()
        {
            return Rzeka.Pluck<UserData>(
                who: this,
                spell: Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Select(x => new UserData("Ali", "Roofwalking Cat", (int)x)));
        }

        protected IDisposable Loom_UserData_To_UserWelcomingText()
        {
            return Rzeka.Loom<UserData, UserWelcomingText>(
                who: this,
                spell: userData => userData
                    .Select(dd => new UserWelcomingText($"Hi {dd.Name}! Ur fav number is <<{dd.FavNumber}>>, a {dd.Zodiac}")));
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
