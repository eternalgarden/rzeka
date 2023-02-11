using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using UnityEngine;

namespace Rzeka
{

    /*

    ? Looming Scrolls Considerations

    Looming Scroll will always have *at least* one dependency
                    and always producing *one* type of matter.

    * Hot vs Cold looms
    * 
    TODO basically all t
    */
    public abstract class LoomingScroll<TOut> : TLoomingScroll<TOut>
        where TOut : TMatter
    {
        public Guid Guid { get; }
        public object Who { get; }
        public Library Library { get; }
        public SpellSchool SpellSchool => SpellSchool.Looming;
        public string Title => $"{Who.GetType().Name}'s Looming of {typeof(TOut).Name}";
        public TSpell ThisAsBase  { get; }
        public TBindingSpell ThisAsBinding { get; }
        public TConjuringSpell<TOut> ThisAsConjuring { get; }

        IObservable<TOut> TConjuringSpell<TOut>.Conjuring { get; set; }

        IObservable<TOut> TConjuringSpell<TOut>.CreateConjuring()
        {
            return CreateConjuring();
        }

        public ISubject<SpellOccurence> SpellStream { get; }
        public ISubject<MatterOccurence> MatterStream { get; }
        public CollectibleDisposable CollectionDisposable { get; set; }
        public Type ConjuredType => typeof(TOut);
        bool TSpell.IsChanneling { get; set; }

        // public bool WasCast => _conjurerLibraryToken is not null; // TODO rework maybe as 'IsActive' alon with the OnLostMana thing
        
        public abstract Dictionary<Type, bool> SatisfiedRequirements { get; }
        protected abstract IObservable<TOut> CreateConjuring();
        
        IDisposable _conjurerLibraryToken;

        public LoomingScroll(
            object who,
            Library library,
            ISubject<SpellOccurence> spellStream, 
            ISubject<MatterOccurence> matterStream)
        {
            Who = who;
            Library = library;
            Guid = Guid.NewGuid();

            SpellStream = spellStream;
            MatterStream = matterStream;

            ThisAsBase = this;
            ThisAsBinding = this;
            ThisAsConjuring = this;
        }

        protected void InitializeLooming()
        {
            ThisAsConjuring.InitializeConjuringSpell();
            ThisAsBinding.InitializeBindingSpell();
        }


        public void Cast()
        {
            if (_conjurerLibraryToken is not null) throw new Exception("Was already cast 🦇");
            if (ThisAsConjuring.Conjuring is null) throw new Exception("Conjuring is null");
            
            Debug.Log("get get got got");

            _conjurerLibraryToken = Library.RegisterConjurer<TOut>(ThisAsConjuring.Conjuring, this);
        }
        
        // TODO VERY IMPORTANT, CHECK IF SPELLS ARE BEING DISPOSED CORRECTLY
        public virtual void Dispose()
        {
            UnregisterConjurerFromLibrary();
            CollectionDisposable.Dispose();
            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
        }

        void TBindingSpell.OnLostMana()
        {
            UnregisterConjurerFromLibrary();
        }

        public ReplaySubject<bool> BindingHasMana { get; } = new();

        void UnregisterConjurerFromLibrary()
        {
            _conjurerLibraryToken?.Dispose();
            _conjurerLibraryToken = null;
        }
    }
}