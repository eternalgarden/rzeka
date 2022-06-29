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

namespace Rzeka
{
    /* 🌊 ---- ---- */
    
    public class RenamedAttribute : Attribute
    {
        private readonly string originalName;

        public RenamedAttribute(string originalName)
        {
            this.originalName = originalName;
        }
    }
    
    /* ---- ---- ⛺ */
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 11 June 2022 🌊 */