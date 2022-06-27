/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using Rzeka;
using Rzeka.Stream;
using UnityEngine;

namespace Rzeka.Examples
{
    /* 🌊 ---- ---- */

    class KeyPressManager : MonoBehaviour
    {
        [SerializeField] GameController gc;
        [SerializeField] KeyCode[] specialCodes;

        void Start()
        {
            // -------------

            Observable.EveryUpdate()
                .Select(_ =>
                {
                    bool foundAny = false;

                    KeyCode[] pressedSpecialKeys = new KeyCode[specialCodes.Length];

                    int pressedIndex = 0;

                    for (int i = 0; i < specialCodes.Length; i++)
                    {
                        KeyCode currentCode = specialCodes[i];

                        if (Input.GetKeyDown(currentCode))
                        {
                            pressedSpecialKeys[pressedIndex] = currentCode;
                            foundAny = true;
                            pressedIndex++;
                        }
                    }

                    return (foundAny: foundAny, codes: pressedSpecialKeys);
                })
                .Where(o => o.foundAny)
                .Subscribe(o =>
                {

                    for (int i = 0; i < o.codes.Length; i++)
                    {
                        KeyCode currentCode = o.codes[i];

                        SpecialKeyPressedEvent e = new();

                        // e.Initialize(
                        //     gift: new SimpleGift<KeyCode>(currentCode),
                        //     context: this,
                        //     circumstances: // TODO a generic root event
                        // );

                        gc.Fillory.Consider(e);
                    }

                });

            // -------------
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */