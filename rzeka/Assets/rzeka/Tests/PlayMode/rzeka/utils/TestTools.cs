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
    public class TestTools
    {
        readonly ITestableRzeka Rzeka;

        public TestTools(ITestableRzeka rzeka)
        {
            Rzeka = rzeka;
        }

        public static void AssertEqual<T>(T expected, T actual)
        {
            Assert.AreEqual(expected, actual, $"Expected: {expected} while actual: {actual}.");
        }

        
        public IDisposable Pluck_UserData(out ConjuringScroll<UserData> scroll, int count = 1)
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
                    }),
                scroll: out scroll);
        }

        public IDisposable Pluck_UserData(out ConjuringScroll<UserData> scroll, params string[] names)
        {
            return Rzeka.Pluck<UserData>(
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
                    }),
                scroll: out scroll);
        }

        public IDisposable Pluck_ANumber(out ConjuringScroll<ANumber> scroll, params int[] numbers)
        {
            return Rzeka.Pluck<ANumber>(
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
                    }),
                scroll: out scroll);
        }

        public IDisposable Pluck_AName(out ConjuringScroll<AName> scroll, params string[] names)
        {
            return Rzeka.Pluck<AName>(
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
                    }),
                scroll: out scroll);
        }

        public IDisposable Loom_ANumber_To_AName(out LoomingScroll_1<ANumber,AName> scroll)
        {
            return Rzeka.Loom<ANumber, AName>(
                who: this,
                spell: number => number
                    .Select(i => new AName($"{i.Number}")),
                scroll: out scroll);
        }

        public IDisposable Loom_AName_To_UserData(out LoomingScroll_1<AName,UserData> scroll)
        {
            return Rzeka.Loom<AName, UserData>(
                who: this,
                spell: aName => aName
                    .Select(i => new UserData(i.Name, "whos that hottie?", 5)),
                scroll: out scroll);
        }

        public IDisposable Pluck_UserData(int count = 1)
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

        public IDisposable Pluck_UserData(params string[] names)
        {
            return Rzeka.Pluck<UserData>(
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

        public IDisposable Pluck_ANumber(params int[] numbers)
        {
            return Rzeka.Pluck<ANumber>(
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

        public IDisposable Pluck_AName(params string[] names)
        {
            return Rzeka.Pluck<AName>(
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

        public IDisposable Pluck_UserDataInterval()
        {
            return Rzeka.Pluck<UserData>(
                who: this,
                spell: Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Select(x => new UserData("Ali", "Roofwalking Cat", (int)x)));
        }

        public IDisposable Loom_UserData_To_UserWelcomingText()
        {
            return Rzeka.Loom<UserData, UserWelcomingText>(
                who: this,
                spell: userData => userData
                    .Select(dd => new UserWelcomingText($"Hi {dd.Name}! Ur fav number is <<{dd.FavNumber}>>, a {dd.Zodiac}")));
        }


        
        //
        // ⛺ ─── Weave ───────────────────────────────────────────────────
        //
        #region Weave
        
        public IDisposable Weave_ANumber(Action<ANumber> onNext = null)
        {
            onNext ??= _ => { };

            return Rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext));
        }

        public IDisposable Weave_ANumber(out AlteringScroll<ANumber> scroll, Action<ANumber> onNext = null)
        {
            onNext ??= _ => { };

            return Rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext),
                scroll: out scroll);
        }

        public IDisposable Weave_AName(Action<AName> onNext = null)
        {
            onNext ??= _ => { };

            return Rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext));
        }

        public IDisposable Weave_AName(out AlteringScroll<AName> scroll, Action<AName> onNext = null)
        {
            onNext ??= _ => { };

            return Rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext),
                scroll: out scroll);
        }
        
        
        public IDisposable Weave_UserData(Action<UserData> onNext = null)
        {
            onNext ??= _ => { };

            return Rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext));
        }
        
        
        public IDisposable Weave_UserData(out AlteringScroll<UserData> scroll, Action<UserData> onNext = null)
        {
            onNext ??= _ => { };

            return Rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext),
                scroll: out scroll);
        }

        public IDisposable Weave_UserWelcomingText(Action<UserWelcomingText> onNext = null)
        {
            onNext ??= _ => { };

            return Rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext));
        }
        
        #endregion // ---------------------------------- Weave -------------------------

    }
    // -------------
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 05 November 2022 🌊 */