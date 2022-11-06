using System;
using System.Collections.Generic;
using System.Reactive.Joins;
using System.Reactive.Linq;

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
            TheLibrary library,
            Eris eris) : base(who, library, eris)
        {
            this.spell = spell;
        }

        public override Type[] Requirements { get; } =
        {
            typeof(T),
            typeof(Y)
        };

        public override Dictionary<Type, bool> AvailableIngredientsDictionary { get; } = new(1)
        {
            { typeof(T), false },
            { typeof(Y), false }
        };

        IDisposable _noManaObserverContract;
        protected override IObservable<Q> GetConjuring()
        {
            IObservable<T> ingredientT = GetIngredient<T>();
            IObservable<Y> ingredientY = GetIngredient<Y>();

            IObservable<Glyph<T,Y>> observable = ingredientT
                .CombineLatest(ingredientY)
                .Skip(1)
                .Select(anon => new Glyph<T, Y>() { one = anon.First, two = anon.Second });

            return spell.Invoke(observable);
        }

        public override void Dispose() 
        {
            base.Dispose();

            _noManaObserverContract?.Dispose();
        }
    }
}