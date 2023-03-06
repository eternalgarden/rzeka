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
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.TestTools;

namespace Rzeka.Tests
{
    internal class TestTools
    {
        readonly ITestableRzeka _rzeka;

        public TestTools(ITestableRzeka rzeka)
        {
            _rzeka = rzeka;
        }

        public static void AssertEqual<T>(T expected, T actual)
        {
            Assert.AreEqual(expected, actual, $"Expected: {expected} while actual: {actual}.");
        }
        
        public static void AssertEqual<T>(T expected, T actual, IEqualityComparer<T> comparer)
        {
            Assert.AreEqual(expected, actual, $"Expected: {expected} while actual: {actual}.", comparer);
        }

        public IDisposable Strand_ANumber_Synchronous(params int[] numbers)
        {
            return _rzeka.Strand<ANumber>(
                who: this,
                spell: Observable
                    .Create<ANumber>(observer =>
                    {
                        for (int i = 0; i < numbers.Length; i++)
                        {
                            observer.OnNext(new ANumber(numbers[i]));
                        }

                        return Disposable.Empty;
                    }));
        }

        public IDisposable Strand_AName_Synchronous(params string[] names)
        {
            return _rzeka.Strand<AName>(
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
        
        // public IDisposable Strand_ANumber_Interval(double milliseconds, params int[] numbers)
        // {
        //     return _rzeka.Strand<ANumber>(
        //         who: this,
        //         spell: Observable
        //             .Interval(TimeSpan.FromMilliseconds(milliseconds))
        //             .Select<ANumber>(observer =>
        //             {
        //                 for (int i = 0; i < numbers.Length; i++)
        //                 {
        //                     observer.OnNext(new ANumber(numbers[i]));
        //                 }
        //
        //                 return Disposable.Empty;
        //             }));
        // }

        public IDisposable Loom_ANumber_To_AName(out LoomingScroll_1<ANumber,AName> scroll)
        {
            return _rzeka.Loom<ANumber, AName>(
                who: this,
                spell: number => number
                    .Select(i => new AName($"{i.Number}")),
                scroll: out scroll);
        }

        public IDisposable Loom_AName_To_UserData(out LoomingScroll_1<AName,UserData> scroll)
        {
            return _rzeka.Loom<AName, UserData>(
                who: this,
                spell: aName => aName
                    .Select(i => new UserData(i.Name, "whos that hottie?", 5)),
                scroll: out scroll);
        }
        
        public IDisposable Strand_ArbitraryMatter1(params string[] strings)
        {
            return _rzeka.Strand<ArbitraryMatter1>(
                who: this,
                spell: Observable
                    .Create<ArbitraryMatter1>(observer =>
                    {
                        for (int i = 0; i < strings.Length; i++)
                        {
                            observer.OnNext(new ArbitraryMatter1(strings[i]));
                        }

                        observer.OnCompleted();

                        return Disposable.Empty;
                    }));
        }
        
        public IDisposable Strand_ArbitraryMatter2(params string[] strings)
        {
            return _rzeka.Strand<ArbitraryMatter2>(
                who: this,
                spell: Observable
                    .Create<ArbitraryMatter2>(observer =>
                    {
                        for (int i = 0; i < strings.Length; i++)
                        {
                            observer.OnNext(new ArbitraryMatter2(strings[i]));
                        }

                        observer.OnCompleted();

                        return Disposable.Empty;
                    }));
        }

        public IDisposable Pluck_UserDataInterval()
        {
            return _rzeka.Strand<UserData>(
                who: this,
                spell: Observable
                    .Interval(TimeSpan.FromSeconds(1))
                    .Select(x => new UserData("Ali", "Roofwalking Cat", (int)x)));
        }

        public IDisposable Loom_UserData_To_UserWelcomingText()
        {
            return _rzeka.Loom<UserData, UserWelcomingText>(
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

            return _rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext));
        }

        public IDisposable Weave_ANumber(out AlteringScroll<ANumber> scroll, Action<ANumber> onNext = null)
        {
            onNext ??= _ => { };

            return _rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext),
                scroll: out scroll);
        }

        public IDisposable Weave_AName(Action<AName> onNext = null)
        {
            onNext ??= _ => { };

            return _rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext));
        }

        public IDisposable Weave_AName(out AlteringScroll<AName> scroll, Action<AName> onNext = null)
        {
            onNext ??= _ => { };

            return _rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext),
                scroll: out scroll);
        }
        
        
        public IDisposable Weave_UserData(Action<UserData> onNext = null)
        {
            onNext ??= _ => { };

            return _rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext));
        }
        
        
        public IDisposable Weave_UserData(out AlteringScroll<UserData> scroll, Action<UserData> onNext = null)
        {
            onNext ??= _ => { };

            return _rzeka.Weave(
                who: this,
                spell: Observer.Create(
                    onNext: onNext),
                scroll: out scroll);
        }

        public IDisposable Weave_UserWelcomingText(Action<UserWelcomingText> onNext = null)
        {
            onNext ??= _ => { };

            return _rzeka.Weave(
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