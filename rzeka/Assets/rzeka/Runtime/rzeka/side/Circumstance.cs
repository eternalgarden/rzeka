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
using Rzeka;

namespace Rzeka
{
    /* 🌊 ---- ---- */
    
    public class Roots
    {
        public static readonly object CORE = new { name = "Core Circumstance, The Root Of Creation, Mother of Dragons, Khalisi Sugar Mommy" };
        public static readonly ThoughtBase USER = new UserThought();
        public static readonly ThoughtBase NULL = new NullThought();
    }

    public class UserThought : ThoughtBase
    {
        public override string Description => "Yes, it was you!";
    }

    public class NullThought : ThoughtBase
    {
        public override string Description => "Null thought.";
    }

    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 06 July 2022 🌊 */