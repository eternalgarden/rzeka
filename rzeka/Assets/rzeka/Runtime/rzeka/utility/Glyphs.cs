/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Rzeka
{
    public struct Glyph<T1, T2>
        where T1 : TMatter
        where T2 : TMatter
    {
        // -------------

        public T1 One { get; set; }
        public T2 Two { get; set; }

        // -------------
    }
    
    public struct Glyph<T1, T2, T3>
        where T1 : TMatter
        where T2 : TMatter
    {
        // -------------

        public T1 One { get; set; }
        public T2 Two { get; set; }
        public T3 Three { get; set; }

        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 05 November 2022 🌊 */