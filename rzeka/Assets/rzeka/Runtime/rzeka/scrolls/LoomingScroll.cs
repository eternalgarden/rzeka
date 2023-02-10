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
        public bool WasCast => _libraryToken is not null; // TODO rework maybe as 'IsActive' alon with the OnLostMana thing

        public abstract Dictionary<Type, bool> SatisfiedRequirements { get; }
        
        IDisposable _libraryToken;

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

        protected abstract IDisposable CastSpell();

        public void Cast()
        {
            if (WasCast is true) throw new Exception("Was already cast 🦇");
            if (ThisAsBinding.HasMana is false) throw new Exception("No mana to cast 😼");
            
            // replace that with a registration to the library
            _libraryToken = CastSpell();

            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Cast);
        }

        public virtual void Dispose()
        {
            _libraryToken?.Dispose();
            CollectionDisposable.Dispose();
            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
        }

        void TBindingSpell.OnLostMana()
        {
            _libraryToken.Dispose();
            _libraryToken = null;
        }
    }
}