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

    */
    public abstract class LoomingScroll<Q> : ILoomingScroll<Q>
        where Q : TMatter
    {
        protected TheLibrary library;
        protected Eris eris;
        protected object who;

        IObservable<Q> _observableSpell;

        public Guid Guid { get; }
        public object Who => who;
        public string Title => $"Looming of {typeof(Q).Name}";
        public Type ConjuredType => typeof(Q);
        public bool IsCastable => (this as TBindingScroll).AreAllIngredientsProvided;
        public bool WasCast => ConjuredSpell is not null;

        public abstract Type[] Requirements { get; }
        public abstract Dictionary<Type, bool> AvailableIngredientsDictionary { get; }

        // TODO hide those shouldnt be public
        public virtual BehaviorSubject<bool> HasMana { get; }= new(false);

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
                if (_observableSpell is not null) throw new Exception("Was already cast, oik!");

                _observableSpell = value;
            }
        }

        public LoomingScroll(
            object who,
            TheLibrary library,
            Eris eris)
        {
            Guid = Guid.NewGuid();
            this.who = who;
            this.library = library;
            this.eris = eris;
        }

        public void Cast()
        {
            if (WasCast is true) throw new Exception("Was already cast 🦇");
            if (IsCastable is false) throw new Exception("Not castable 😼");

            ConjuredSpell = GetConjuring();
        }

        protected abstract IObservable<Q> GetConjuring();

        protected IObservable<X> GetIngredient<X>() where X : TMatter
        {
            IObservable<X> ingredient = library.AskForIngredient<X>();

            return ingredient
                .Do(eris.GetReceivalsObserver<X>(this));
        }

        public virtual void Dispose()
        {
            library = null;
            eris = null;
            who = null;
            _observableSpell = null;
        }
    }
}