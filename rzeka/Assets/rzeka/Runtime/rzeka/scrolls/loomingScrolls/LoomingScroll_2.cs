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
        readonly Func<Pattern<T,Y>, IObservable<Q>> spell;

        public LoomingScroll_2(
            object who,
            Func<Pattern<T,Y>, IObservable<Q>> spell,
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
            IObservable<T> ingredientT = library.AskForIngredient<T>();
            IObservable<Y> ingredientY = library.AskForIngredient<Y>();

            Pattern<T,Y> pattern = ingredientT.And(ingredientY);

            return spell.Invoke(pattern);
        }

        public override void Dispose() 
        {
            base.Dispose();

            _noManaObserverContract?.Dispose();
        }
    }
}