using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    public interface ISerlializableBindingSpell : ISerializableSpell
    {
        bool hasMana { get; set; }
        Dictionary<string, SerializableStranding[]> ingredients { get; set; }
    }

    public interface TBindingScroll : TScrollBase
    {
        Dictionary<Type, List<IConjuringScroll>> Ingredients { get; }
        TBindingScroll ThisAsBinding { get; }

        protected void OnLostMana();

        //
        // ⛺ ─── Default Imlementation ───────────────────────────────────────────────────
        //
        #region Default Imlementation

        bool WouldPossiblyLike<T>() where T : TMatter
        {
            return Ingredients.ContainsKey(typeof(T));
        }

        bool WouldPossiblyLike(Type type)
        {
            return Ingredients.ContainsKey(type);
        }

        bool HasMana => Ingredients.All(x => x.Value.Count > 0);

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
                // .Where(matter => matter.Equals(default(T)) is false) // TODO understand why does it make a difference in case of behaviour subvjects
                .Do( // TODO Maybe add on completed for any reason?
                    // * It's safe to use .Do modifier here since besically the ingredient given here
                    // * will be prepended to whatever following sequence of operators
                    // * Reminder, appending is dangerous (since it can cool a hot observable), prepending isnt
                    onNext: matter => ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Received),
                    onError: err => ThisAsBase.SendMatterExceptionOccurence(err));

            // return erisTouchedIngredient;
            return ingredient;
           
           /* ---- ---- 🌠 */
        }

        #endregion // ---------------------------------- Default Imlementation -------------------------


        //
        // ⛺ ─── Registrations ───────────────────────────────────────────────────
        //
        #region Registrations

        void InitializeBindingSpell()
        {
            InitializeSpellBase();

            RegisterPostCreationManaCheck();
            CastListen();
            ForgottenListen();
        }

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

        private void CastListen()
        {
            CollectionDisposable += SpellStream
                .Where(i => i.Source.SpellSchool is SpellSchool.Looming or SpellSchool.Stranding)
                .Where(i => ThisAsBinding.WouldPossiblyLike((i.Source as IConjuringScroll).ConjuredType))
                .Where(i => i.SpellOccurenceCategory == SpellOccurenceCategory.Cast)
                .Subscribe(i =>
                {
                    IConjuringScroll conjurableScroll = i.Source as IConjuringScroll;
                    Type conjuredType = conjurableScroll.ConjuredType;
                    Ingredients[conjuredType].Add(conjurableScroll);

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

        private void ForgottenListen()
        {
            CollectionDisposable += SpellStream
                .Where(i => i.Source.SpellSchool is SpellSchool.Looming or SpellSchool.Stranding)
                .Where(i => ThisAsBinding.WouldPossiblyLike((i.Source as IConjuringScroll).ConjuredType))
                .Where(i => i.SpellOccurenceCategory is SpellOccurenceCategory.Forgotten or SpellOccurenceCategory.NoMana)
                .Subscribe(i =>
                {
                    IConjuringScroll conjurableScroll = i.Source as IConjuringScroll;
                    Type conjuredType = conjurableScroll.ConjuredType;
                    Ingredients[conjuredType].Remove(conjurableScroll);

                    if (WasCast)
                    {
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