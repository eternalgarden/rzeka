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
    public abstract class Glyph
    {
        public abstract Guid[] AsCircumstance();
    }

    public class Glyph<T, Y> : Glyph
        where T : TMatter
        where Y : TMatter
    {
        // -------------

        public T one { get; set; }
        public Y two { get; set; }

        public override Guid[] AsCircumstance()
        {
            return new Guid[2] { one.Guid, two.Guid };
        }

        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 05 November 2022 🌊 */