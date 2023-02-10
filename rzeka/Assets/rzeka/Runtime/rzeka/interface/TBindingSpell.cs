using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{

  public interface TBindingSpell : TSpell
    {
        Dictionary<Type, bool> SatisfiedRequirements { get; }
        TBindingSpell ThisAsBinding { get; }

        protected void OnLostMana(); // TODO

        //
        // ⛺ ─── Default Imlementation ───────────────────────────────────────────────────
        //
        #region Default Imlementation

        bool HasMana => SatisfiedRequirements.All(x => x.Value is true);

        void SetRequirementSatisfied(Type type, bool satisfied)
        {
            if (RequiresIngredient(type) is false) Debug.LogError("HUH");

            SatisfiedRequirements[type] = satisfied;
            
            if (WasCast)
            {
                if (HasMana is true) return;
                
                OnLostMana();
                ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.NoMana);
            }
            else
            {
                if (HasMana)
                {
                    ThisAsBase.Cast();
                }
            }
        }
        
        bool RequiresIngredient(Type key)
        {
            return SatisfiedRequirements.ContainsKey(key);
        }
        
        bool IsMissingIngredient(Type key)
        {
            return RequiresIngredient(key) && SatisfiedRequirements[key] is false;
        }

        IObservable<T> GetObservableIngredient<T>() where T : TMatter
        {
           /* ⭐ ---- ---- */
           
            IObservable<T> ingredient = Library.GetConjurer<T>();
            
            var erisTouchedIngredient = ingredient
                .Do( // TODO Maybe add on completed for any reason?
                    // * It's safe to use .Do modifier here since besically the ingredient given here
                    // * will be prepended to whatever following sequence of operators
                    // * Reminder, appending is dangerous (since it can cool a hot observable), prepending isnt
                    onNext: matter => ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Received),
                    onError: err => ThisAsBase.SendMatterExceptionOccurence(err));

            return erisTouchedIngredient;
           
           /* ---- ---- 🌠 */
        }

        #endregion // ---------------------------------- Default Imlementation -------------------------


        //
        // ⛺ ─── Registrations ───────────────────────────────────────────────────
        //
        #region Registrations

        void InitializeBindingSpell()
        {
            /* ⭐ ---- ---- */
            
            InitializeSpellBase();

            // * Binding Spell Registrations
            RegisterPostCreationManaCheck();
            ListenForAppearingIngredients();
            ListenForFadingIngredients();
            
            // TODO ADD CHECK FOR SATISFIED INGREDIENTS
            
            /* ---- ---- 🌠 */
        }
        
        // nd
        /// <summary>
        /// AFter being created, check after a designated time whether the spell (this one)
        /// has been provided "mana" / necessary ingredients.
        /// </summary>
        private void RegisterPostCreationManaCheck()
        {
            var activeKeys = SatisfiedRequirements
                .Where(kvp => Library.IsStreamAvailable(kvp.Key))
                .Select(kvp => kvp.Key)
                .ToArray(); // TODO why if you remove this there is an error in c2_NoMana_Cast_Weave test

            foreach (Type key in activeKeys)
            {
                if (Library.IsStreamAvailable(key)) SetRequirementSatisfied(key, true);
            }
            
            if (WasCast is false) ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.NoMana);
        }

        /// <summary>
        /// Listen for relevant (providing ingredients of necessary type) conjurers cast.
        /// </summary>
        private void ListenForAppearingIngredients()
        {
            CollectionDisposable += SpellStream
                .Where(i => i.Source.IsConjuring())
                .Where(i => i.SpellOccurenceCategory == SpellOccurenceCategory.Cast)
                .Select(i => i.Source as IConjuringSpell)
                .Where(i => ThisAsBinding.IsMissingIngredient(i.ConjuredType))
                .Subscribe(i =>
                {
                    Type conjuredType = i.ConjuredType;

                    ThisAsBinding.SetRequirementSatisfied(conjuredType, true);

                });
        }

        /// <summary>
        /// Listen for relevant conjurers being forgotten or knocked out of mana.
        /// </summary>
        private void ListenForFadingIngredients()
        {
            CollectionDisposable += SpellStream
                .Where(i => i.Source.IsConjuring())
                .Where(i => i.SpellOccurenceCategory is SpellOccurenceCategory.Forgotten or SpellOccurenceCategory.NoMana)
                .Select(i => i.Source as IConjuringSpell)
                .Where(i => ThisAsBinding.RequiresIngredient(i.ConjuredType))
                .Subscribe(i =>
                {
                    Type conjuredType = i.ConjuredType;
                    
                    if (Library.IsStreamAvailable(conjuredType) is false)
                    {
                        ThisAsBinding.SetRequirementSatisfied(conjuredType, false);
                    }
                });
        }

        #endregion // ---------------------------------- Registrations -------------------------
    }
}