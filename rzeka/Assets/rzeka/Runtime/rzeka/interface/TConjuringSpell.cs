using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    public interface TConjuringSpell<TOut> : IConjuringSpell where TOut : TMatter
    {
        TConjuringSpell<TOut> ThisAsConjuring { get; }
        
        // ND: this is getting obsolete because of library being the provider of conjurings
        // nd or is it?
        
        /// <summary>
        /// This hides an important architecture decision that the conjuring spells may only be cast once.
        /// To re-cast one it's scroll would have to be disposed and summoned again, unless it has gotten out of mana and then it is be provided with it again.
        /// TODO CONSIDER ADDING ReCast() INTERFACE METHOD
        /// </summary>
        IObservable<TOut> Conjuring { get; set; }
        
        // ND: obsolete because of the above

        // bool TSpell.WasCast => ConjuredSpell is not null;
        protected IObservable<TOut> CreateConjuring();

        //
        // ⛺ ─── Conjurer Registrations ───────────────────────────────────────────────────
        //
        #region Conjurer Registrations

        void InitializeConjuringSpell()
        {
            /* ⭐ ---- ---- */
            
            InitializeSpellBase();
            ListenForIntroductions();

            Conjuring = CreateConjuring();

            /* ---- ---- 🌠 */
        }
        
        private void ListenForIntroductions()
        {
            /* ⭐ ---- ---- */
            
            // ND: this goes away because bindings request their ingredients from the library
            
            // since provide ingredient will be killed this is obsolete
            // CollectionDisposable += SpellStream
            //     .Where(_ => this.WasCast)
            //     .Where(i => i.SpellOccurenceCategory is SpellOccurenceCategory.Created)
            //     .Where(i => i.Source.SpellSchool is SpellSchool.Looming or SpellSchool.Weaving)
            //     .Select(i => i.Source as TBindingSpell)
            //     .Where(scroll => scroll.WouldPossiblyLike<TOut>())
            //     .Subscribe(scroll => {
            //         scroll.ProvideIngredient<TOut>(this);
            //     });
            //
            /* ---- ---- 🌠 */
        }
        
        #endregion // ---------------------------------- Conjurer Registrations -------------------------
    }
}