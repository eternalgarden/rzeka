using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    public interface ISerializableConjuringSpell : ISerializableSpell
    {
        string ConjuredType { get; set; }
    }

    [Serializable]
    public struct SerializableConjuring : ISerializableConjuringSpell
    {
        public string ConjuredType { get; set; }
        public Guid Guid { get; set; }
        public string Title { get; set; }
        public SpellType SpellType { get; set; }
        public object Who { get; set; }
        public bool WasCast { get; set; }
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
                .Where(i => i.SpellType is SpellType.Looming or SpellType.Weaving)
                .Where(i => i.SpellOccurenceCategory is SpellOccurenceCategory.Created)
                .Select(i => i.Scroll as TBindingScroll)
                .Where(scroll => scroll.WouldPossiblyLike<Q>())
                .Subscribe(scroll => {
                    Debug.Log($"provided");
                    
                    scroll.ProvideIngredient<Q>(this);
                });
            
            /* ---- ---- 🌠 */
        }
        
        #endregion // ---------------------------------- Conjurer Registrations -------------------------
    }
}