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

        //
        // ⛺ ─── Default Imlementation ───────────────────────────────────────────────────
        //
        #region Default Imlementation

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
            
            // Debug.Log($"<color=green>well?</color>");

            var erisTouchedIngredient = ingredient
                .Do( // TODO Maybe add on completed for any reason?
                    // * It's safe to use .Do modifier here since besically the ingredient given here
                    // * will be prepended to whatever following sequence of operators
                    // * Reminder, appending is dangerous (since it can cool a hot observable), prepending isnt
                    onNext: matter => ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Received),
                    onError: ThisAsBase.SendMatterExceptionOccurence);

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
            
            // * Binding Spell Registrations
            // RegisterPostCreationManaCheck();
            // ListenForAppearingIngredients();
            // ListenForFadingIngredients();

            using IDisposable initialManaCheck = Eris
                .ManaProvideableObservable
                .Subscribe(manainfo =>
                {
                    var keys = SatisfiedRequirements
                        .Select(kvp => kvp.Key)
                        .ToArray();
                    
                    foreach (Type type in keys)
                    {
                        CheckIfHasMana(type, manainfo.IsManaOfTypeAvailable(type));
                    }
                    
                }, onError: ex     => Debug.LogError("errer") );
            
            if (HasMana is false) ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.NoMana);

            Q += Eris
                .ManaProvideableObservable
                .Skip(1) // handled above
                .Where(manainfo => RequiresIngredient(manainfo.LastChangedType))
                .Subscribe(
                    onNext: manainfo =>
                {
                    Type type = manainfo.LastChangedType;
                    CheckIfHasMana(type, manainfo.IsManaOfTypeAvailable(type));
                }, onError: ex     => Debug.LogError("errer") );
            
            
            /* ---- ---- 🌠 */
        }
        
        void CheckIfHasMana(Type type, bool satisfied)
        {
            if (RequiresIngredient(type) is false) Debug.LogError("HUH");

            SatisfiedRequirements[type] = satisfied;
            
            bool hasMana = SatisfiedRequirements.All(x => x.Value is true);
            
            // Debug.Log($"oik {HasMana} {hasMana}");
            if (HasMana == hasMana) return;
            HasMana = hasMana;
            
            ThisAsBase.SendSpellOccurence(HasMana is false
                ? SpellOccurenceCategory.NoMana
                : SpellOccurenceCategory.HasMana); // has mana rename
        }

        #endregion // ---------------------------------- Registrations -------------------------
    }
}