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
                    SpecialKeyPressEvent e = new();

                    e.Initialize(
                        gift: new SimpleGift<KeyCode>(keyCode),
                        context: this,
                        circumstances: new UnknownStrand()
                    );

                    Rzeka.Pluck(e);
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