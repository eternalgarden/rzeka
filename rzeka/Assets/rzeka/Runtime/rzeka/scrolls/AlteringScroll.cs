using System;
using System.Collections.Generic;
    using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    [Serializable]
    public class AlteringScroll<T> : TAlteringScroll
        where T : TMatter
    {
        readonly IObserver<T> spell;
        IDisposable _subscriptionDisposable;

        public Guid Guid { get; }
        public CollectibleDisposable CollectionDisposable { get; set; }
        public bool WasCast { get; private set; }
        public object Who { get; }
        public TScrollBase ThisAsBase  { get; }
        public TBindingScroll ThisAsBinding { get; }
        public ISubject<SpellOccurence> SpellStream { get; }
        public ISubject<MatterOccurence> MatterStream { get; }
        public SpellSchool SpellSchool => SpellSchool.Weaving;
        public string Title => $"{Who.GetType().Name}'s Weaving of {typeof(T).Name}";

        public Dictionary<Type, List<IConjuringScroll>> Ingredients { get; } = new(1)
        {
            { typeof(T), new List<IConjuringScroll>() }
        };


        public AlteringScroll(object who, IObserver<T> spell, ISubject<SpellOccurence> spellStream, ISubject<MatterOccurence> matterStream)
        {
            this.spell = spell;

            Guid = Guid.NewGuid();
            Who = who;
            SpellStream = spellStream;
            MatterStream = matterStream;
            ThisAsBase = this;
            ThisAsBinding = this;

            ThisAsBinding.InitializeBindingSpell();
        }

        
        //
        // ⛺ ─── Public ───────────────────────────────────────────────────
        //
        #region Public
        
        public void Cast()
        {
            try
            {
                ExecuteCast();

                ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Cast);
            }
            catch (Exception ex)
            {
                // var wispd = new SpellOccurence
                // {
                //     SpSchoolype = SpellType.Weaving,
                //     SpellOccurenceCategory = SpellOccurenceCategory.Wispd,
                //     Scroll = this,
                //     Luggage = new ExceptionalLuggage() { Exception = ex }
                // };
                
                ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Wispd);

                // todo well well
                Debug.LogError(ex.Message);
                Debug.LogError(ex.StackTrace);

                throw ex;

            }
        }

        void TBindingScroll.OnLostMana()
        {
             _subscriptionDisposable.Dispose();
        }

        public void Dispose()
        {
            if (WasCast) _subscriptionDisposable.Dispose(); // TODO check this

            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
            CollectionDisposable.Dispose();
        }
        
        #endregion // ---------------------------------- Public -------------------------

        void ExecuteCast()
        {
            // todo add this check
            // if (ThisAsBinding.IsCastable is false) throw new Exception("messed up");

            var ingredient = ThisAsBinding.GetObservableIngredient<T>();

            if (ingredient is null) throw new Exception($"Missing ingredient of type {typeof(T)}");

            _subscriptionDisposable = ingredient.Subscribe(spell);

            WasCast = true;
        }
    }
}