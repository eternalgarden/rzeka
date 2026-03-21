/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/



// TODO move to internal namespace along with all the Spell scripts
namespace Rzeka
{
    public static class ScrollExtensions
    {
        // -------------
        
        public static bool IsConjuring(this TSpell scroll)
        {
            bool isConjuring = scroll.SpellSchool is SpellSchool.Looming or SpellSchool.Stranding;
            return isConjuring;
        }
    
        public static bool IsConjuring(this TSpell scroll, out TStrandingSpell asStranding)
        {
            bool isConjuring = scroll.SpellSchool is SpellSchool.Looming or SpellSchool.Stranding;
            asStranding = isConjuring ? scroll as TStrandingSpell : null;
            return isConjuring;
        }

        public static bool IsBinding(this TSpell scroll)
        {
            bool isBinding = scroll.SpellSchool is SpellSchool.Looming or SpellSchool.Weaving;
            return isBinding;
        }
    
        public static bool IsBinding(this TSpell scroll, out TBindingSpell asBinding)
        {
            bool isBinding = scroll.SpellSchool is SpellSchool.Looming or SpellSchool.Weaving;
            asBinding = isBinding ? scroll as TBindingSpell : null;
            return isBinding;
        }
    
        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 26 January 2023 🌊 */