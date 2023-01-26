/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

// TODO move to internal namespace along with all the Spell scripts
namespace Rzeka
{
    public static class ScrollExtensions
    {
        // -------------
        
        public static bool IsConjuring(this TScrollBase scroll)
        {
            bool isConjuring = scroll.SpellSchool is SpellSchool.Looming or SpellSchool.Stranding;
            return isConjuring;
        }
    
        public static bool IsConjuring(this TScrollBase scroll, out IConjuringScroll asConjuring)
        {
            bool isConjuring = scroll.SpellSchool is SpellSchool.Looming or SpellSchool.Stranding;
            asConjuring = isConjuring ? scroll as IConjuringScroll : null;
            return isConjuring;
        }

        public static bool IsBinding(this TScrollBase scroll)
        {
            bool isBinding = scroll.SpellSchool is SpellSchool.Looming or SpellSchool.Weaving;
            return isBinding;
        }
    
        public static bool IsBinding(this TScrollBase scroll, out TBindingScroll asBinding)
        {
            bool isBinding = scroll.SpellSchool is SpellSchool.Looming or SpellSchool.Weaving;
            asBinding = isBinding ? scroll as TBindingScroll : null;
            return isBinding;
        }
    
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 26 January 2023 🌊 */