using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    public interface ISerializableConjuringSpell : ISerializableSpell
    {
        string conjuredType { get; set; }
    }

    [Serializable]
    public struct SerializableStranding : ISerializableConjuringSpell
    {
        public string conjuredType { get; set; }
        public Guid guid { get; set; }
        public string title { get; set; }
        public SpellSchool spellSchool { get; set; }
        public string whosName { get; set; }
        public bool wasCast { get; set; }
    }

    public interface IConjuringScroll : TScrollBase
    {
        Type ConjuredType { get; }
    }

    public interface TConjuringScroll<Q> : IConjuringScroll where Q : TMatter
    {
        TConjuringScroll<Q> ThisAsConjuring { get; }

        /// <summary>
        /// This hides an important architecture decision that the conjuring spells may only be cast once.
        /// To re-cast one it's scroll would have to be disposed and summoned again, unless it has gotten out of mana and then it is be provided with it again.
        /// TODO CONSIDER ADDING ReCast() INTERFACE METHOD
        /// </summary>
        IObservable<Q> ConjuredSpell { get; }

        bool TScrollBase.WasCast => ConjuredSpell is not null;
        
        //
        // ⛺ ─── Conjurer Registrations ───────────────────────────────────────────────────
        //
        #region Conjurer Registrations

        void InitializeConjuringSpell()
        {
            InitializeSpellBase();

            ListenForIntroductions();
        }
        
        private void ListenForIntroductions()
        {
            /* ⭐ ---- ---- */

            CollectionDisposable += SpellStream
                .Where(_ => this.WasCast)
                .Where(i => i.Source.SpellSchool is SpellSchool.Looming or SpellSchool.Weaving)
                .Where(i => i.SpellOccurenceCategory is SpellOccurenceCategory.Created)
                .Select(i => i.Source as TBindingScroll)
                .Where(scroll => scroll.WouldPossiblyLike<Q>())
                .Subscribe(scroll => {
                    scroll.ProvideIngredient<Q>(this);
                });
            
            /* ---- ---- 🌠 */
        }
        
        #endregion // ---------------------------------- Conjurer Registrations -------------------------
    }
}