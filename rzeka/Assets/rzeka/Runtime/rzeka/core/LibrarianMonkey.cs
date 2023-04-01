/* 
      |\      _,,,---,,_
ZZZzz /,`.-'`'    -.  ;-;;,_
     |,4-  ) )-,_. ,\ (  `'-'
    '---''(_/--'  `-'\_)
*/
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Rzeka
{
    public class LibrarianMonkey
    {
        // -------------

        public IObservable<SpellOccurence> SpellOccurences { get; }
        public IObservable<MatterOccurence> MatterOccurences { get; }

        public LibrarianMonkey(IObservable<SpellOccurence> spellOccurences, IObservable<MatterOccurence> matterOccurences)
        {
            SpellOccurences = spellOccurences;
            MatterOccurences = matterOccurences;
        }

        public StrandingSpell<Q> CreateConjuring<Q>(object who, IObservable<Q> spell) where Q : TMatter
        {
            //  

            return null;
        }

        public LoomingSpell<Q> CreateLoom<T, Q>(object who, Func<IObservable<T>, IObservable<Q>> spell) where T : TMatter where Q : TMatter
        {
            // LoomingScroll_1<T, Q> newScroll = new(who, spell, TheLibrary, Eris);

            return null;
        }

        public AlteringScroll<T> CreateWeaving<T>(object who, IObserver<T> spell) where T : TMatter
        {
            // LoomingScroll_1<T, Q> newScroll = new(who, spell, TheLibrary, Eris);

            return null;
        }

        // -------------
    }
}
/* dreamy guardian ASCII kitty by Felix Lee, found at asciiart.eu 🐱‍👤 */
/* 06 November 2022 🌊 */