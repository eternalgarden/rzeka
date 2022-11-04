using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Rzeka
{
    [Serializable]
    public class LoomingScroll_2<T, Y, Q> : LoomingScroll<Q>
        where Q : TMatter
        where T : TMatter
        where Y : TMatter
    {
        readonly Func<IObservable<T>, IObservable<Q>> spell;

        public LoomingScroll_2(
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
            IObservable<T> ingredient = library.AskForIngredient<T>();

            T lastCircumstance = default(T);

            var erisTouchedIngredient = ingredient
                .DistinctUntilChanged(keySelector: next => next.Guid)
                .Do(eris.GetReceivalsObserver<T>(this))
                .Do(onNext: next =>
                {
                    lastCircumstance = next;
                });

            return spell
                .Invoke(erisTouchedIngredient)
                .Do(onNext: next =>
                {
                    // * So that circumstances are set automatically if not specified directly
                    if (next.HasCircumstances() is false)
                    {
                        next.SetCircumstances(lastCircumstance);
                    }
                });
        }

        public override void Dispose() 
        {
            base.Dispose();

            _noManaObserverContract?.Dispose();
        }
    }
}