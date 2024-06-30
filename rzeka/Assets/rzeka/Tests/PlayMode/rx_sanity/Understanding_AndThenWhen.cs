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
        public IEnumerator a_andthenwhen_has_a_ziplike_blocking_behaviour()
        {
            // -------------

            var obs1 = Observable.Return(1);
            var obs2 = Observable.Interval(TimeSpan.FromSeconds(0.1)); // 0.1

            var pattern = obs1.And(obs2);
            var plan = pattern.Then((x, tick) => new { x, tick });

            var mergedObservable = Observable
                .When(plan);
                
            int receivals = 0;

            using var oik = mergedObservable.Subscribe(_ => receivals++);
            
            yield return EditorFakeDelay(2f);
            // yield return null;
            // yield return new WaitForSeconds(0.35f); // ! 0.35 meaning 3 ticks going through

            // * notice we are still getting only one receival
            // it's because obs1 gave only one tick and it "blocked" obs2
            // this is a desired behaviour in .Zip operator
            AssertEqual(1, receivals);

            // -------------
        }

        [UnityTest]
        public IEnumerator b_andthenwhen_has_a_ziplike_blocking_behaviour_proof_possibly()
        {
            // -------------

            var obs1 = Observable
                .Return(2)
                .SelectMany(i => Observable.Range(0, i));

            var obs2 = Observable.Interval(TimeSpan.FromSeconds(0.1)); // 0.1

            var pattern = obs1.And(obs2);
            var plan = pattern.Then((x, tick) => new { x, tick });

            var when = Observable
                .When(plan);

            int receivals = 0;

            using var oik = when.Subscribe(_ => receivals++);

            yield return new WaitForSeconds(0.35f); // ! 0.35 meaning 3 ticks going through

            // * now there are two receivals
            // interval still ticked three times
            // but obs1 ticked twice
            // this confirms AndThenWhen to have an implicit .Zip-like behaviour
            AssertEqual(2, receivals);

            // -------------
        }

        [UnityTest]
        public IEnumerator c_andthenwhen_has_a_ziplike_blocking_behaviour_proof_definitely()
        {
            // -------------

            var obs1 = Observable
                .Return(10) // ! in this case obs1 will tick more often
                .SelectMany(i => Observable.Range(1, i));

            var obs2 = Observable.Interval(TimeSpan.FromSeconds(0.1)); // 0.1

            var pattern = obs1.And(obs2);
            var plan = pattern.Then((o1, tick) => new { o1, tick });

            var when = Observable
                .When(plan);

            int lastValueFromObs1 = -1;

            using var oik = when.Subscribe(a => lastValueFromObs1 = a.o1);

            yield return new WaitForSeconds(0.35f); // ! 0.35 meaning 3 ticks going through

            // * obs1 would go untill 10 but still it's last receival is 3
            // this is because interval tick blocked it's emission
            // it proves definitely andthenwhen ziplike behaviour
            // and it's rejection as a stream-merging operator for rzeka
            AssertEqual(3, lastValueFromObs1);

            // -------------
        }

        [UnityTest]
        public IEnumerator d_andthenwhen_throws_error_when_one_of_its_observables_throws_it()
        {
            /* ⭐ ---- ---- */

            var obs1 = Observable.Interval(TimeSpan.FromSeconds(0.1)); // will keep running
            var obs2 = Observable.Throw<Exception>(new()); // * will throw error immediately

            var plan = obs1.And(obs2).Then((o1,o2)=>(o1,o2));
            var when = Observable.When(plan);

            bool gotException = false;
            using var oik = when.Subscribe(
                onNext: _ => {}, 
                onError: ex => gotException = true);
            
            yield return null;

            AssertEqual(true, gotException);
            
            /* ---- ---- 🌠 */
        }

        [UnityTest]
        public IEnumerator e_andthenwhen_does_NOT_complete_untill_ALL_of_its_observables_complete()
        {
            /* ⭐ ---- ---- */

            var obs1 = Observable.Interval(TimeSpan.FromSeconds(0.1)); // will keep running
            var obs2 = Observable.Return(1); // * this will complete immediately

            var plan = obs1.And(obs2).Then((o1,o2)=>(o1,o2));
            var when = Observable.When(plan);

            bool gotCompletion = false;
            using var oik = when.Subscribe(
                onNext: _ => {}, 
                onCompleted: () => gotCompletion = true);
            
            yield return null;

            AssertEqual(false, gotCompletion); // EXPECT FALSE
            
            /* ---- ---- 🌠 */
        }

        [UnityTest]
        public IEnumerator f_andthenwhen_DOES_complete_WHEN_ALL_of_its_observables_complete()
        {
            /* ⭐ ---- ---- */

            var obs1 = Observable.Timer(TimeSpan.FromSeconds(0.1)); // * will produce value and complete after 0.1 seconds
            var obs2 = Observable.Return(1); // * this will complete immediately

            var plan = obs1.And(obs2).Then((o1,o2)=>(o1,o2));
            var when = Observable.When(plan);

            bool gotCompletion = false;
            using var oik = when.Subscribe(
                onNext: _ => {}, 
                onCompleted: () => gotCompletion = true);
            
            yield return new WaitForSeconds(0.15f); // 0.15 will give it enough time

            AssertEqual(true, gotCompletion); // EXPECT TRUE
            
            /* ---- ---- 🌠 */
        }

        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 04 November 2022 🌊 */