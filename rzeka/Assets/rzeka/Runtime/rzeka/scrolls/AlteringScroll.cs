using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{
    [Serializable]
    public class AlteringScroll<T1> : TWeavingSpell
        where T1 : TMatter
    {
        readonly IObserver<T1> _spell;
        IDisposable _subscriptionDisposable;

        public Guid Guid { get; }
        public Library Library { get; }
        public CollectibleDisposable CollectionDisposable { get; set; }
        public object Who { get; }
        public TSpell ThisAsBase  { get; }
        public TBindingSpell ThisAsBinding { get; }
        public ISubject<SpellOccurence> SpellStream { get; }
        public ISubject<MatterOccurence> MatterStream { get; }
        public SpellSchool SpellSchool => SpellSchool.Weaving;
        public string Title => $"{Who.GetType().Name}'s Weaving of {typeof(T1).Name}";
        
        readonly Dictionary<Type, bool> _satisfiedRequirements = new(1)
        {
            { typeof(T1), false },
        };

        bool _isChanneling;

        public Dictionary<Type, bool> SatisfiedRequirements => _satisfiedRequirements;
        
        public AlteringScroll(object who, Library library, IObserver<T1> spell, ISubject<SpellOccurence> spellStream, ISubject<MatterOccurence> matterStream)
        {
            _spell = spell;

            Guid = Guid.NewGuid();
            Library = library;
            Who = who;
            SpellStream = spellStream;
            MatterStream = matterStream;
            ThisAsBase = this;
            ThisAsBinding = this;

            ThisAsBinding.InitializeBindingSpell();
            
            ExecuteCast(); // ! this is weird but currently weavings should cast immediately aswell
        }

        
        //
        // ⛺ ─── Public ───────────────────────────────────────────────────
        //
        #region Public
        
        public void Cast()
        {
            try
            {
                // ExecuteCast(); 
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
                // Debug.LogError(ex.StackTrace);

                throw ex;

            }
        }

        void TBindingSpell.OnLostMana()
        {
             // _subscriptionDisposable.Dispose();
             // _subscriptionDisposable = null; // TODO name this clearer
        }

        public ReplaySubject<bool> BindingHasMana { get; } = new();

        bool TSpell.IsChanneling
        {
            get => _isChanneling;
            set => _isChanneling = value;
        }

        public void Dispose()
        {
            _subscriptionDisposable?.Dispose(); // TODO check this
            CollectionDisposable.Dispose();
            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
        }
        
        #endregion // ---------------------------------- Public -------------------------

        void ExecuteCast()
        {
            // todo add this check
            // if (ThisAsBinding.IsCastable is false) throw new Exception("messed up");

            var ingredient = ThisAsBinding.GetObservableIngredient<T1>();

            if (ingredient is null) throw new Exception($"Missing ingredient of type {typeof(T1)}");
            
            _subscriptionDisposable = ingredient
                .Subscribe(_spell);
        }
    }
}