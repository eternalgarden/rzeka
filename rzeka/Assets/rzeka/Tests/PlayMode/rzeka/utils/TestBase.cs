/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Rzeka.Tests
{
    public abstract class TestBase
    {
        protected IRzeka Rzeka;
        protected CollectibleDisposable Q; // TODO Remove references to this its bugged as fukkkkkkk
        
        [UnitySetUp]
        public virtual IEnumerator Setup()
        {
            // -------------

            Rzeka = new SpringRiver();
            Q = new();

            yield return null;

            // -------------
        }

        [UnityTearDown]
        public virtual IEnumerator Teardown()
        {
            // -------------

            Rzeka.Dispose();
            Q ?.Dispose();

            yield return null;

            // -------------
        }

        protected static void AssertEqual<T>(T expected, T actual)
        {
            Assert.AreEqual(expected, actual, $"Expected: {expected} while actual: {actual}.");
        }

        protected IDisposable Pluck_UserData(int count = 1)
        {
            return Rzeka.Strand<UserData>(
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

        protected IDisposable Pluck_UserData(params string[] names)
        {
            return Rzeka.Strand<UserData>(
                who: this,
                spell: Observable
                    .Create<UserData>(observer =>
                    {
                        for (int i = 0; i < names.Length; i++)
                        {
                            observer.OnNext(new UserData(names[i], "whos that hottie?", i));
                        }

                        observer.OnCompleted();

                        return Disposable.Empty;
                    }));
        }

        protected IDisposable Pluck_ANumber(params int[] numbers)
        {
            return Rzeka.Strand<ANumber>(
                who: this,
                spell: Observable
                    .Create<ANumber>(observer =>
                    {
                        for (int i = 0; i < numbers.Length; i++)
                        {
                            observer.OnNext(new ANumber(numbers[i]));
                        }

                        observer.OnCompleted();

                        return Disposable.Empty;
                    }));
        }

        protected IDisposable Pluck_AName(params string[] names)
        {
            return Rzeka.Strand<AName>(
                who: this,
                spell: Observable
                    .Create<AName>(observer =>
                    {
                        for (int i = 0; i < names.Length; i++)
                        {
                            observer.OnNext(new AName(names[i]));
                        }

                        observer.OnCompleted();

                        return Disposable.Empty;
                    }));
        }

        protected IDisposable Pluck_UserDataInterval()
        {
            return Rzeka.Strand<UserData>(
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
    // -------------
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 05 November 2022 🌊 */