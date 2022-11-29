using System;
using System.Collections.Generic;
using System.Reactive.Joins;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Rzeka
{
    [Serializable]
    public class LoomingScroll_2<T, Y, Q> : LoomingScroll<Q>
        where Q : TMatter
        where T : TMatter
        where Y : TMatter
    {
        readonly Func<IObservable<Glyph<T, Y>>, IObservable<Q>> spell;

        public LoomingScroll_2(
            object who,
            Func<IObservable<Glyph<T, Y>>, IObservable<Q>> spell,
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
            { typeof(Y), new List<IConjuringScroll>() }
        };

        protected override IObservable<Q> GetConjuring()
        {
            IObservable<T> ingredientT = ThisAsBinding.GetObservableIngredient<T>();
            IObservable<Y> ingredientY = ThisAsBinding.GetObservableIngredient<Y>();

            IObservable<Glyph<T,Y>> observable = ingredientT
                .CombineLatest(ingredientY)
                .Skip(1)
                .Select(anon => new Glyph<T, Y>() { one = anon.First, two = anon.Second });

            return spell.Invoke(observable);
        }
    }
}