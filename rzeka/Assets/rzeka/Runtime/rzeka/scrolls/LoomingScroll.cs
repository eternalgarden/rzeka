using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

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
    public abstract class LoomingScroll<Q> : TLoomingScroll<Q>
        where Q : TMatter
    {
        public Guid Guid { get; }
        public object Who { get; }
        public Library Library { get; }
        public SpellSchool SpellSchool => SpellSchool.Looming;
        public string Title => $"{Who.GetType().Name}'s Looming of {typeof(Q).Name}";
        public TSpell ThisAsBase  { get; }
        public TBindingSpell ThisAsBinding { get; }
        public TConjuringSpell<Q> ThisAsConjuring { get; }
        public ISubject<SpellOccurence> SpellStream { get; }
        public ISubject<MatterOccurence> MatterStream { get; }
        public CollectibleDisposable CollectionDisposable { get; set; }

        public Type ConjuredType => typeof(Q);
        public bool WasCast => ConjuredSpell is not null; // TODO rework maybe as 'IsActive' alon with the OnLostMana thing

        // TODO A NECESSARY RE-CAST ONCE MANA IS PROVIDED
        // COULD THIS BE AUTOMATIC WITHOUT INPUT FROM THE LIBRARY THAT SPELLS CAST THEMSELVES ONCE PROVIDED WITH MANA
        public IObservable<Q> ConjuredSpell
        {
            get
            {
                return _observableSpell;
            }
            private set
            {
                _observableSpell = value;
            }
        }
        public abstract HashSet<Type> NewIngredients { get; }
        public abstract Dictionary<Type, List<IConjuringSpell>> Ingredients { get; }
        
        IObservable<Q> _observableSpell;

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

        protected abstract IObservable<Q> GetConjuring();

        public void Cast()
        {
            if (WasCast is true) throw new Exception("Was already cast 🦇");
            if (ThisAsBinding.HasMana is false) throw new Exception("No mana to cast 😼");
            
            // replace that with a registration to the library
            ConjuredSpell = GetConjuring();

            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Cast);
        }

        public virtual void Dispose()
        {
            _observableSpell = null;

            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
            CollectionDisposable.Dispose();
        }

        void TBindingSpell.OnLostMana()
        {
            ConjuredSpell = null;
        }
    }
}