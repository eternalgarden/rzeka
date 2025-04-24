using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;
using System.Threading;
using Rzeka.Unirx;

namespace Rzeka.Examples
{
    public class Zip : LoomingMono
    {
        //[Button(".SomeMethod()")]
        void SomeMethod()
        {

        }

        [InfoBox("Press enter to print values of the interval.")]
        [SerializeField] bool RunPlaymodeExample;

        protected override void OnEnable()
        {
            if (RunPlaymodeExample is false) return;

            base.OnEnable();

            var interval = Observable
                .Interval(TimeSpan.FromSeconds(0.5f));

            var returnListener = UnityObservable
                .EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Return));

            q += interval
                .Zip(second: returnListener) // ZIP
                .Subscribe(comb =>
                {
                    Debug.Log($"Returned at x: {comb.First}");
                });

            q += UnityObservable
                .EveryUpdate()
                .Where(_ => Input.GetKeyDown(KeyCode.Return))
                .Subscribe(_ => Application.Quit());
        }
    }
}
