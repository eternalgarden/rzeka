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
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NUnit.Framework;
using Rzeka.Tests.Integration;
using UnityEngine;
using UnityEngine.TestTools;
using System.Linq;

namespace Rzeka.Tests.Rx
{
    public class Understanding_CombineLatest : TestBase
    {
        // -------------
        
        [UnityTest]
        public IEnumerator a_will_need_all_of_observables_to_emit_at_least_once()
        {
            // -------------

            var obs1 = Observable.Timer(TimeSpan.FromSeconds(0.2)); // 0.2 wont make through
            var obs2 = Observable.Timer(TimeSpan.FromSeconds(0.1)); // 0.1 will make through

            var combine = obs1.CombineLatest(obs2);

            int receivals = 0;
            using var oik = combine.Subscribe(_ => receivals++);

            yield return new WaitForSeconds(0.15f); // ! 0.15

            // since obs1 wont make through on time there wont be a single receival
            AssertEqual(0, receivals);

            // -------------
        }

        [UnityTest]
        public IEnumerator b_will_emit_each_time_one_of_the_observables_emit()
        {
            // -------------

            var obs1 = Observable.Return(1);
            var obs2 = Observable.Interval(TimeSpan.FromSeconds(0.1)); // 0.1 tick

            var combine = obs1.CombineLatest(obs2);

            int receivals = 0;
            using var oik = combine.Subscribe(_ => receivals++);

            yield return new WaitForSeconds(0.35f); // ! 0.35 meaning 3 ticks going through

            AssertEqual(3, receivals);

            // -------------
        }

        [UnityTest]
        public IEnumerator c_buffered_last_observable_will_keep_same_value()
        {
            // -------------
            
            // * example setup is exactly the same as b_ above

            var obs1 = Observable.Return(1);
            var obs2 = Observable.Interval(TimeSpan.FromSeconds(0.1)); // 0.1 tick

            var combine = obs1.CombineLatest(obs2);

            bool valueOfObs1WasAlways1 = true;

            using var oik = combine.Subscribe(combine => {
                if (combine.First != 1) valueOfObs1WasAlways1 = false; // should be 1 every time
            });

            yield return new WaitForSeconds(0.35f); // ! 0.35 meaning 3 ticks going through

            AssertEqual(true, valueOfObs1WasAlways1);

            // -------------
        }

        [UnityTest]
        public IEnumerator d_consecutive_values_of_active_observable_will_create_new_combinations()
        {
            // -------------
            
            // * example setup is exactly the same as b_ above

            var obs1 = Observable.Return(1);
            var obs2 = Observable.Interval(TimeSpan.FromSeconds(0.1)); // 0.1 tick, interval emits consecutive long values starting wiht 0

            var combine = obs1.CombineLatest(obs2);

            List<long> intervalValues = new();

            using var oik = combine.Subscribe(combine => {
                intervalValues.Add(combine.Second); // will have three different values as the interval keeps going
            });

            yield return new WaitForSeconds(0.35f); // ! 0.35 meaning 3 ticks going through

            bool areEqual = new long[] { 0,1,2 }.SequenceEqual(intervalValues.ToArray());
            
            AssertEqual(true, areEqual);

            // -------------
        }

        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 04 November 2022 🌊 */