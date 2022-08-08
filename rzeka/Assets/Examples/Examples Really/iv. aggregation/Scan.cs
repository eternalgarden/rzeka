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
    /*
     * http://introtorx.com/Content/v1.0.10621.0/07_Aggregation.html#Scan
     *  It is probably worth pointing out that you use Scan with TakeLast() to produce Aggregate.
        source.Aggregate(0, (acc, current) => acc + current);
        //is equivalent to 
        source.Scan(0, (acc, current) => acc + current).TakeLast();
     */
    public class Scan : LoomingMono
    {
        class UniqueCollecter
        {
            public List<int> UniqueNumbers = new();
        }
        
        [Button(".Scan()")]
        void Scan1()
        {
            Func<int> throwADice = () => UnityEngine.Random.Range(0, 10);

            var tenRandomNumbers = Observable
                .Range(0, 10)
                .Select(_ => throwADice.Invoke());

            var scanner = tenRandomNumbers
                .Scan(
                    seed: new UniqueCollecter(),
                    accumulator: (acc, x) =>
                    {
                        if (acc.UniqueNumbers.Contains(x) is false)
                        {
                            acc.UniqueNumbers.Add(x);
                        }

                        return acc;
                    });

            using var distinctprinter = scanner
                .Distinct(collector => collector.UniqueNumbers.Count)
                .Subscribe(collecter =>
                {
                    Debug.Log($"Colecter has now {collecter.UniqueNumbers.Count} elements.");
                });
        }

        //[InfoBox("")]
        //[SerializeField] bool RunPlaymodeExample = true;

        //protected override void OnEnable()
        //{
        //    if (RunPlaymodeExample is false) return;
        //    else
        //    {
        //        base.OnEnable();

        //        q += UnityObservable
        //            .EveryUpdate()
        //            .Where(_ => Input.GetKeyDown(KeyCode.Return))
        //            .Subscribe(_ =>
        //            {
        //                //
        //            });
        //    }
        //}
    }
}
