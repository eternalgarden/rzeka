using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

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
            ISubject<SpellOccurence> spellStream, 
            ISubject<MatterOccurence> matterStream) : base(who, spellStream, matterStream) 
        {
            this.spell = spell;

            ThisAsBinding.InitializeBindingSpell();
            ThisAsConjuring.InitializeConjuringSpell();
        }

        public override Dictionary<Type, List<IConjuringScroll>> Ingredients { get; } = new(1)
        {
            { typeof(T), new List<IConjuringScroll>() },
        };

        // LOOMING SCROLL IS BASICALLY ONLY CAST FROM THE LIBRARY AS AskForIngredient 
        // WHEN IT WAS BLOCKED
        // IF IT WASNT BLOCKED IT WILL BE CAST IMMEDIATELY
        // TODO WHAT IF IT ISNT USED BY ANY WEAVING, WHY WOULD IT BE CAST THEN
        // TODO IT NEEDS CLEANING 
        protected override IObservable<Q> GetConjuring()
        {
            // TODO So before I thought I had this perfect idea to handle circumstances
            // TODO automatically here when the spell is being cast
            // * however.. THE SAME PROBLEM AS WITH PUSHING, 
            // * A HOT OBSERVABLE WILL BE FROZEN WITH such .DO
            // ! For now circumstances will have to be assigned manually
            // ? Could this be handled with an additonal interface/contract below IObservable?

            IObservable<T> ingredientT = ThisAsBinding.GetObservableIngredient<T>();

            return spell.Invoke(ingredientT);
        }
    }
}