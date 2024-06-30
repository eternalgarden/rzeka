/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        
        public static void AssertEqual<T>(T expected, T actual, Func<T,T,bool> comparer)
        {
            bool areEqual = comparer.Invoke(expected, actual);
            Assert.AreEqual(true, areEqual, $"Expected: {GetPrintable<T>(expected)} while actual: {GetPrintable<T>(actual)}.");
        }

        static string GetPrintable<T>(T expected)
        {
            string result = "";
            if (expected is IEnumerable enumerable)
            {
                result += "{ ";
                foreach (var x in enumerable)
                {
                    result += $"{x} ";
                }
                result += " }";
            }
            else
            {
                result = expected.ToString();
            }

            return result;
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

        public IDisposable Loom_ANumber_To_AName(out LoomingSpell1<ANumber,AName> scroll)
        {
            return _rzeka.Loom<ANumber, AName>(
                who: this,
                spell: number => number
                    .Select(i => new AName($"{i.Number}")),
                scroll: out scroll);
        }

        public IDisposable Loom_AName_To_UserData(out LoomingSpell1<AName,UserData> scroll)
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

        public IDisposable Strand_ArbitraryStatefulMatter1(params int[] states)
        {
            return _rzeka.Strand<ArbitraryStatefulMatter1>(
                who: this,
                spell: Observable
                    .Create<ArbitraryStatefulMatter1>(observer =>
                    {
                        foreach (int t in states)
                        {
                            observer.OnNext(new ArbitraryStatefulMatter1(t));
                        }

                        observer.OnCompleted();

                        return Disposable.Empty;
                    }));
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
        
        
        //
        // ⛺ ─── Generic ───────────────────────────────────────────────────
        //
        #region Generic

        public IDisposable Strand_Return<T>() where T : TMatter, new()
        {
            // TODO REALLY NEED A FACTORY FOR MATTER
            // returning new T() will skip all guid setting and important matter info
            var t = new T();

            return _rzeka.Strand<T>(
                who: this,
                spell: Observable
                    .Return<T>(t));
        }
        
        public IDisposable Weave<T>(Action<T> onNext = null) where T : TMatter
        {
            onNext ??= _ => { };

            return _rzeka.Weave<T>(
                who: this,
                spell: Observer.Create(
                    onNext: onNext));
        }

        #endregion

    }
    // -------------
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 05 November 2022 🌊 */