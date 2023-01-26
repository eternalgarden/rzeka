using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{

  public interface TBindingScroll : TScrollBase
    {
        Dictionary<Type, List<IConjuringScroll>> Ingredients { get; }
        TBindingScroll ThisAsBinding { get; }

        protected void OnLostMana(); // TODO

        //
        // ⛺ ─── Default Imlementation ───────────────────────────────────────────────────
        //
        #region Default Imlementation

        bool HasMana => Ingredients.All(x => x.Value.Count > 0);

        bool WouldPossiblyLike<T>() where T : TMatter
        {
            return Ingredients.ContainsKey(typeof(T));
        }

        bool WouldPossiblyLike(Type type)
        {
            return Ingredients.ContainsKey(type);
        }

        // * this will be useful in testing
        IConjuringScroll[] GetIngredients<X>() where X : TMatter
        {
            if (!Ingredients.ContainsKey(typeof(X))) throw new Exception("Ugh, it doesnt serve this kind of type");

            return Ingredients[typeof(X)].ToArray();
        }

        void ProvideIngredient<X>(IConjuringScroll ingredient) where X : TMatter
        {
            if (ingredient is null) throw new Exception("Cannot provide null ingredient");
            if (!Ingredients.ContainsKey(typeof(X))) throw new Exception("Cannot accept ingredient of this type");

            Ingredients[typeof(X)].Add(ingredient);

            if (WasCast)
            {
                throw new NotImplementedException("proovide multiple ingredients");
            }
            else
            {
                if (HasMana)
                {
                    ThisAsBase.Cast();
                }
            }
        }

        IObservable<T> GetObservableIngredient<T>() where T : TMatter
        {
           /* ⭐ ---- ---- */
           
            var ingredientsT = Ingredients[typeof(T)];
            IObservable<T> ingredient = null;

            if (ingredientsT.Count > 1)
            {
                // TODO
                throw new NotImplementedException("Implement multiple providers of type");
            }
            else if (ingredientsT.Count == 1)
            {
                if ((ingredientsT[0] as TConjuringScroll<T>) is null) throw new Exception("wtf");
                ingredient = (ingredientsT[0] as TConjuringScroll<T>).ConjuredSpell;
            }
            else
            {
                throw new Exception($"Binding is missing ingredients of type: {typeof(T)}");
            }

            if (ingredient is null) throw new Exception($"Something went wrong for Conjurer {ingredientsT[0].GetType()} of type {typeof(T)}.");

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
            
            /* ---- ---- 🌠 */
        }
        
        /// <summary>
        /// AFter being created, check after a designated time whether the spell (this one)
        /// has been provided "mana" / necessary ingredients.
        /// </summary>
        private void RegisterPostCreationManaCheck()
        {
            IDisposable postCreationManaCheck = null;

            postCreationManaCheck = Observable
                .Timer(TimeSpan.FromSeconds(TScrollBase.POST_CREATION_MANA_CHECK_DELAY))
                .Subscribe(_ =>
                {
                    if (WasCast is false)
                    {
                        if (HasMana) throw new Exception("Spell should have already been cast in that case");

                        ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.NoMana);
                    }

                    postCreationManaCheck.Dispose();
                });
        }

        /// <summary>
        /// Listen for relevant (providing ingredients of necessary type) conjurers cast.
        /// </summary>
        private void ListenForAppearingIngredients()
        {
            CollectionDisposable += SpellStream
                .Where(i => i.Source.IsConjuring())
                .Where(i => i.SpellOccurenceCategory == SpellOccurenceCategory.Cast)
                .Select(i => i.Source as IConjuringScroll)
                .Where(i => ThisAsBinding.WouldPossiblyLike(i.ConjuredType))
                .Subscribe(i =>
                {
                    Type conjuredType = i.ConjuredType;
                    Ingredients[conjuredType].Add(i);

                    if (WasCast)
                    {
                        // todo handling multiple sources
                        // ? ReCast
                    }
                    else if (HasMana)
                    {
                        Cast();
                    }
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
                .Select(i => i.Source as IConjuringScroll)
                .Where(i => ThisAsBinding.WouldPossiblyLike(i.ConjuredType))
                .Subscribe(i =>
                {
                    Type conjuredType = i.ConjuredType;
                    Ingredients[conjuredType].Remove(i);

                    if (WasCast)
                    {
                        // TODO this needs to be validated for multiple providers
                        if (ThisAsBinding.HasMana is false)
                        {
                            OnLostMana();

                            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.NoMana);
                        }
                    }
                });
        }

        #endregion // ---------------------------------- Registrations -------------------------
    }
}