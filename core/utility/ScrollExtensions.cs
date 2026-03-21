/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/



using System;
using System.Reactive.Linq;

// TODO move to internal namespace along with all the Spell scripts
namespace Rzeka
{
    public static class ScrollExtensions
    {
        // -------------

        /// <summary>
        /// Combines two streams into a tuple, emitting whenever either fires (CombineLatest).
        /// Use when both streams are triggers — e.g. a display that updates when health OR shield changes.
        /// </summary>
        public static IObservable<(T1, T2)> CombineWith<T1, T2>(
            this IObservable<T1> source,
            IObservable<T2> other)
            => source.CombineLatest(other, (a, b) => (a, b));

        /// <summary>
        /// Pairs each emission of source with the latest value from context (WithLatestFrom).
        /// Use when source is the trigger and context is just "what's the current state of X".
        /// Note: silently drops source emissions that arrive before context has emitted.
        /// </summary>
        public static IObservable<(T1, T2)> WithContext<T1, T2>(
            this IObservable<T1> source,
            IObservable<T2> context)
            => source.WithLatestFrom(context, (a, b) => (a, b));
        
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