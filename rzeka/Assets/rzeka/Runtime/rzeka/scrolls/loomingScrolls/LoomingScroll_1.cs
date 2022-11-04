using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Rzeka
{
    [Serializable]
    public class LoomingScroll_1<T, Q> : LoomingScroll<Q>
        where Q : TMatter 
        where T : TMatter
    {
        readonly Func<IObservable<T>, IObservable<Q>> spell;

        public LoomingScroll_1(
            object who, 
            Func<IObservable<T>, IObservable<Q>> spell, 
            TheLibrary library, 
            Eris eris) : base(who, library, eris)
        {
            this.spell = spell;
        }

        public override Type[] Requirements { get; } =
        {
            typeof(T),
        };

        public override Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
        {
            { typeof(T), false },
        };

        IDisposable _noManaObserverContract;
        IObservable<Q> _observableSpell;

        // LOOMING SCROLL IS BASICALLY ONLY CAST FROM THE LIBRARY AS AskForIngredient 
        // WHEN IT WAS BLOCKED
        // IF IT WASNT BLOCKED IT WILL BE CAST IMMEDIATELY
        // TODO WHAT IF IT ISNT USED BY ANY WEAVING, WHY WOULD IT BE CAST THEN
        // TODO IT NEEDS CLEANING 
        protected override IObservable<Q> GetConjuring()
        {
            if (WasCast) throw new Exception("Was already cast!");
            if ((this as TBindingScroll).IsCastable is false) throw new Exception("Not castable");

            IObservable<T> ingredient = library.AskForIngredient<T>();

            // TODO THIS IS A BIG CONSIDERATION TO MAKE
            // ARE SCROLLS CAST ONLY ONCE, ON LATER REQUESTS AN EXISTING 'CAST' IS PROVIDED
            // ALSO HANDLING SITUATION WHEN ONE OF IT'S INGREDIENTS IS KNOCKED OUT
            // ALSO HANDLING SITUATION WHEN THE CHANNEL IS NO LONGER CLOSED WITH A WEAVING
            // DOES THAT HAVE TO MATTER?
            // ARE SCROLLS CAST IF THE CHANNEL ISNT CLOSED?

            // TODO OKI, SO FAR A CHANGE IN INGREDIENTS WILL BE PROPAGATED FURTHER
            // BUT WHAT IF THIS SCROLL ITSELF IS BECOMING AN INACTIVE/DISPOSED COMPONENT
            // WHERE DOES THE INGREDIENT WATERFALL BEGIN, IT'S NOT CLEAR
            // var noManaNotifier = Observable.Create<Q>(subscribe: observer =>
            // {
            //     return HasMana
            //         .Where(hasMana => hasMana is false)
            //         .Subscribe(onNext: _ =>
            //         {
            //             Debug.Log("throwing no mana in loom");
            //             observer.OnError(new NoManaException());
            //         });
            // });

            // TODO an attempt at circumstances
            T lastCircumstance = default(T);

            var erisTouchedIngredient = ingredient
                .DistinctUntilChanged(keySelector: next => next.Guid) // TODO is this necessary
                .Do(eris.GetReceivalsObserver<T>(this))
                .Do(onNext: next =>
                {
                    // TODO couldn't circumstances be intercepted here?
                    lastCircumstance = next;
                });

            return spell
                .Invoke(erisTouchedIngredient);
                // .Merge(noManaNotifier) // TODO DISABLED
                
                // TODO So before I thought I had this perfect idea to handle circumstances
                // TODO automatically here when the spell is being cast
                // * however.. THE SAME PROBLEM AS WITH PUSHING, 
                // * A HOT OBSERVABLE WILL BE FROZEN WITH such .DO
                // ! For now circumstances will have to be assigned manually
                // ? Could this be handled with an additonal interface/contract below IObservable?
                /*

                Seomthing like 
                
                IChanneling<T> : IObservable<Q>
                {
                    last circumstances would be here
                    then an extension handling circumstances would be possible
                }

                */
                // .Do(onNext: next =>
                // {
                //     // * So that circumstances are set automatically if not specified directly
                //     if (next.HasCircumstances() is false)
                //     {
                //         next.SetCircumstances(lastCircumstance);
                //     }
                // });
        }

        public override void Dispose()
        {
            base.Dispose();
            _noManaObserverContract?.Dispose();
        }
    }
}