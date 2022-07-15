/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Linq;
using System.Reactive.Linq;
using UnityEngine;

namespace RzekaRiver.Examples
{
    /* 🌊 ---- ---- */

    public class MathAndAggregate : MonoBehaviour
    {
        void Start()
        {
            // TODO weird result with default scheduler (silent)
            var just = Observable.Range(0, 3); // 0 + 1 + 2 = 3

            just
                .Aggregate((s, v) =>
                {
                    return s + v;
                }).Subscribe(i =>
                {
                    Debug.Log($"<color=yellow>Some sum inty int: {i}</color>");
                });

        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 18 June 2022 🌊 */