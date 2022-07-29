/*
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)

most of the code straight out copied from @neuecc UniRx project
https://github.com/neuecc/UniRx
*/
using Rzeka;
using UnityEngine;

namespace Examples.Fillory
{
    /* 🌊 ---- ---- */

    public class SpecialKeyPressed : 
            Thought     <SimpleMatter<KeyCode>>
    {
        public override string Description => $"Dreamer pressed a special key {Matter.Content}";
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */