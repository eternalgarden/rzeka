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
        IObservable<Q> _observableSpell;

        public Guid Guid { get; }
        public object Who { get; }
        public SpellType SpellType => SpellType.Looming;
        public string Title => $"{Who.GetType().Name}'s Looming of {typeof(Q).Name}";
        public TScrollBase ThisAsBase  { get; }
        public TBindingScroll ThisAsBinding { get; }
        public TConjuringScroll<Q> ThisAsConjuring { get; }
        public ISubject<SpellOccurence> SpellStream { get; }
        public ISubject<MatterOccurence> MatterStream { get; }
        public CollectibleDisposable CollectionDisposable { get; set; }

        public Type ConjuredType => typeof(Q);
        public bool WasCast => ConjuredSpell is not null;

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

        public abstract Dictionary<Type, List<IConjuringScroll>> Ingredients { get; }

        public LoomingScroll(
            object who, 
            ISubject<SpellOccurence> spellStream, 
            ISubject<MatterOccurence> matterStream)
        {
            Who = who;
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

            ConjuredSpell = GetConjuring();

            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Cast);
        }

        public virtual void Dispose()
        {
            _observableSpell = null;

            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
            CollectionDisposable.Dispose();
        }

        void TBindingScroll.OnLostMana()
        {
            ConjuredSpell = null;
        }
    }
}