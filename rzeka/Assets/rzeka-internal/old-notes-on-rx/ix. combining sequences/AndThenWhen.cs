using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;
using System.Threading;
using Rzeka.Unirx;
using System.Reactive.Joins;

namespace Rzeka.Examples
{
    public class AndThenWhen : LoomingMono
    {
        [Button(".AndThenWhen()")]
        void SomeMethod()
        {
            var one = Observable.Interval(TimeSpan.FromSeconds(1)).Take(5);
            var two = Observable.Interval(TimeSpan.FromMilliseconds(250)).Take(10);
            var three = Observable.Interval(TimeSpan.FromMilliseconds(150)).Take(14);

            // 1. MAKING A PATTERN

            // And doesn't have any overloads, you simply use it to create a Pattern
            // Normally it's a perfect case to just use 'var', but wanted to show underlying type
            Pattern<long, long, long> pattern = one
                .And(two)
                .And(three);


            // 2. RUNNING THE PATTERN THROUGH A SELECTOR

            // Then you run a *s e l e c t o r* .Then on the previous pattern
            // Whats cool is you can use another hard type as a return type or actually make a new anonymous type like in this example
            // Notice I left 'var' here, with an anonymous type you cannot type it explicitly
            var plan = pattern
                .Then((first, second, third) => new { One = first, Two = second, Three = third });

            var zippedSequence = Observable
                .When(plan);

            zippedSequence.Subscribe(
                onNext: comb => Debug.Log(comb),
                onCompleted: () => Debug.Log("Completed"));
        }
    }
}
