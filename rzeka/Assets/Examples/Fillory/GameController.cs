/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using RzekaRiver;
using UnityEngine;

namespace Examples.Fillory
{
    /* 🌊 ---- ---- */

    class GameController : MonoBehaviour
    {
        [SerializeField] Rzeka rzeka;

        void Start()
        {
            // -------------

            // var strand = rzeka
            //     .Weave<SpecialKeyPressed>(this)
            //     .Subscribe(next =>
            //     {
            //         Debug.Log(next.Matter.Content.ToString());
            //     });

            // -------------
        }

        void Update()
        {
            // -------------.
            
            if (rzeka._strands[typeof(SpecialKeyPressed)] is Subject<ThoughtBase> skp)
            {
                Debug.Log($"<color=cyan>{skp.HasObservers}</color>");
            }
            
            // -------------
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */