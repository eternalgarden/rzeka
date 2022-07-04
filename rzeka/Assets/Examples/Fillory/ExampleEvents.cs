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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rzeka.Stream;

namespace Rzeka.Examples
{
    /* 🌊 ---- ---- */

    public class SpecialKeyPressedEvent : StreamGiftEvent<SimpleGift<KeyCode>>
    {
        public override string Description => $"Dreamer pressed a special key {Gift.Content}";
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */