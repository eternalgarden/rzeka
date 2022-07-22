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
using RzekaRiver;
using UnityEngine;

namespace Examples.Fillory
{
    /* 🌊 ---- ---- */
    
    class SpecialKeyManager : MonoBehaviour
    {
        [SerializeField] KeyCode[] specialCodes;
        [SerializeField] Rzeka rzeka;

        IDisposable cat;

        void OnEnable()
        {
            // -------------

            cat = UnityObservable.EveryUpdate()
                .SelectMany(_ =>
                {
                    List<KeyCode> pressedSpecialKeys = new(specialCodes.Length);

                    for (int i = 0; i < specialCodes.Length; i++)
                    {
                        KeyCode currentCode = specialCodes[i];

                        if (Input.GetKeyDown(currentCode))
                        {
                            pressedSpecialKeys.Add(currentCode);
                        }
                    }

                    return pressedSpecialKeys;
                })
                .Subscribe(keyCode =>
                {
                    SpecialKeyPressed e = new();

                    e.Initialize(
                        matter: new SimpleMatter<KeyCode>(keyCode),
                        context: this,
                        circumstances: Roots.USER
                    );

                    rzeka.Pluck(e);
                });

            // -------------
        }

        void OnDisable()
        {
            // -------------

            cat.Dispose();

            // -------------
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */