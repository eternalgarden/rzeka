/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Collections;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Rzeka.Tests.Integration;
using UnityEngine;
using UnityEngine.TestTools;

namespace Rzeka.Tests.Rx
{
    public class Understanding_AndThenWhen : TestBase
    {
        // -------------

        [UnityTest]
        public IEnumerator a_()
        {
            // -------------

            int count = 0;

            var obs1 = Observable.Return(1);
            var obs2 = Observable.Interval(TimeSpan.FromSeconds(0.1));

            var pattern = obs1.And(obs2);
            var plan = pattern.Then((x, tick) => new { x, tick });

            var when = Observable
                .When(plan)
                .Do(_ => count++);

            using var oik = when.Subscribe();

            yield return new WaitForSeconds(0.35f);

            // * since naturally a new anonymous object will be emit each time one 
            // * of it's merged obsservables emits
            AssertEqual(1, count);

            // -------------
        }

        [UnityTest]
        public IEnumerator b_()
        {
            // -------------

            int count = 0;

            var obs1 = Observable
                .Return(3)
                .SelectMany(i => Observable.Range(0, i));

            var obs2 = Observable.Interval(TimeSpan.FromSeconds(0.1));

            var pattern = obs1.And(obs2);
            var plan = pattern.Then((x, tick) => new { x, tick });

            var when = Observable
                .When(plan)
                .Do(_ => count++);

            using var oik = when.Subscribe();

            yield return new WaitForSeconds(0.35f);

            // * since naturally a new anonymous object will be emit each time one 
            // * of it's merged obsservables emits
            AssertEqual(3, count);

            // -------------
        }

        // [UnityTest]
        // public IEnumerator c_()
        // {
        //     // -------------

        //     int count = 0;

        //     var obs1 = Observable
        //         .Return(3)
        //         .SelectMany(i => Observable.Range(0, i));

        //     var obs2 = Observable.Interval(TimeSpan.FromSeconds(0.1));

        //     var pattern = obs1.CombineLatest(obs2, (one, two) => new { one, two });

        //     IObservable<Glyph<int,long>> glyph = pattern
        //         .DistinctUntilChanged(x => new { x.one, x.two })
        //         .Select(anon => new Glyph<int, long>() { one = anon.one, two = anon.two });

        //     using var oik = glyph.Subscribe(_ => count++);

        //     yield return new WaitForSeconds(0.35f);

        //     // * since naturally a new anonymous object will be emit each time one 
        //     // * of it's merged obsservables emits
        //     AssertEqual(3, count);

        //     // -------------
        // }

        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 04 November 2022 🌊 */