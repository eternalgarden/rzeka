/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using NaughtyAttributes;
using UnityEngine;

namespace Rzeka
{
    /* 🌊 ---- ---- */

    public class Example6_Aggregation : MonoBehaviour
    {
        class UniqueCollecter
        {
            public int LastNumber;
            public bool WasLastUnique;
            public List<int> UniqueNumbers = new();
        }

        [Button("B. Observable.Scan()")]
        static void Scan()
        {
            Func<int> throwADice = () => UnityEngine.Random.Range(0, 10);

            var range = Observable
                .Range(0, 10);

            var tenRandomNumbers = range
                .Select(_ => throwADice.Invoke());

            var scanner = tenRandomNumbers
                .Scan(
                    seed: new UniqueCollecter(),
                    accumulator: (collecter, x) =>
                    {
                        collecter.LastNumber = x;

                        if (collecter.UniqueNumbers.Contains(x) is false)
                        {
                            collecter.UniqueNumbers.Add(x);
                            collecter.WasLastUnique = true;
                        }
                        else
                        {
                            collecter.WasLastUnique = false;
                        }

                        return collecter;
                    });

            using var distinctprinter = scanner
                .Distinct(collector => collector.UniqueNumbers.Count)
                .Subscribe(collecter =>
                {
                    Debug.Log($"Colecter has now {collecter.UniqueNumbers.Count} elements.");
                });

            using var listprinter = scanner
                .Aggregate(
                    seed: new { allnumbers = "All numbers: ", uniquenumbers = "Unique numbers" },
                    accumulator: (texts, collector) =>
                    {
                        return new
                        {
                            allnumbers = texts.allnumbers + $" {collector.LastNumber}",
                            uniquenumbers = collector.WasLastUnique
                                ? texts.uniquenumbers
                                : texts.uniquenumbers + $" {collector.LastNumber}"
                        };
                    }
                )
                .Subscribe(
                    // final =>
                );
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 20 July 2022 🌊 */