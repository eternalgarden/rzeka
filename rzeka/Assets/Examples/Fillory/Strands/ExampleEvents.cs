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
using RzekaRiver;
using UnityEngine;

namespace Examples.Fillory
{
    /* 🌊 ---- ---- */

    public class SpecialKeyPressEvent
        : StreamGiftEvent<SimpleGift<KeyCode>>
    {
        public override string Description => $"Dreamer pressed a special key {Gift.Content}";
    }



    public class ConsoleOpenStrand : Gift
    {
        public bool IsConsoleOpen { get; set; }
    }
    
    public class ToggleConsoleEvent
        : StreamGiftEvent<ConsoleOpenStrand>,
            HasDefaultValue<ConsoleOpenStrand>,
            HasMemory
    {

        public override string Description
        {
            get
            {
                if (this.Gift.IsConsoleOpen == true)
                    return "The console is being opened.";
                else
                    return "The console is being closed.";
            }
        }

        public int MemoryCapacity => 1;

        public ConsoleOpenStrand DefaultValue()
        {
            return new ConsoleOpenStrand() { IsConsoleOpen = false };
        }
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 17 June 2022 🌊 */