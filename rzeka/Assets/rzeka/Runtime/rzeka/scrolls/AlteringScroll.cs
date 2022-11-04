using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka
{
    [Serializable]
    public class AlteringScroll<T> : IAlteringScroll 
        where T : TMatter
    {
        readonly IObserver<T> spell;
        readonly TheLibrary library;
        readonly Eris eris;
        readonly object who;
        readonly Guid _guid = Guid.NewGuid();
        IDisposable _subscriptionDisposable;

        public AlteringScroll(object who, IObserver<T> spell, TheLibrary library, Eris eris)
        {
            this.who = who;
            this.spell = spell;
            this.library = library;
            this.eris = eris;
        }

        public Guid Guid => _guid;
        public string Title => $"Weaving of {(this as TBindingScroll).GetBindingScrollCode()}";

        public bool IsCastable
        {
            get
            {
                // TODO So the strong concept here was that Altering scrolls once cast are uncastable
                // TODO What happens if there appears a new or changed provider for the data they seek while they already opearate
                if (WasCast) return false;

                return (this as TBindingScroll).AreAllIngredientsProvided;
            }
        }

        public Type[] Requirements { get; } = new[] { typeof(T) };
        public BehaviorSubject<bool> HasMana { get; } = new(false);

        public Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
        {
            { typeof(T), false }
        };

        public bool WasCast { get; private set; }
        public object Who => who;

        public void Dispose()
        {
            if (WasCast) _subscriptionDisposable.Dispose(); // TODO check this
        }

        public void Cast()
        {
            if ((this as TBindingScroll).IsCastable is false) throw new Exception("messed up");

            // var noManaNotifier = Observable.Create<T>(subscribe: observer =>
            // {
            //     return HasMana
            //         .Where(hasMana => hasMana is false)
            //         .Subscribe(onNext: _ =>
            //         {
            //             Debug.Log("throwing no mana in weave");
            //             observer.OnError(new NoManaException());
            //         });
            // });

            IObservable<T> ingredient = library.AskForIngredient<T>();

            var erisTouchedIngredient = ingredient
                .Do(eris.GetReceivalsObserver<T>(this));

            _subscriptionDisposable = erisTouchedIngredient
                // .Merge(noManaNotifier)
                // .Subscribe(spell.OnNext, err =>
                // {
                //     if (err is NoManaException)
                //     {
                //         // THE SPELL BECOMES INACTIVE/BLOCKED
                //         _subscriptionDisposable.Dispose();
                //     }
                //     else
                //     {
                //         spell.OnError(err);
                //     }
                // }, spell.OnCompleted);
                .Subscribe(spell);

            WasCast = true;
        }
    }
}