using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Rzeka;
public class ShuttleSpell<TIn, TOut> : LoomingSpell<TOut>
    where TIn : IRequest
    where TOut : IResponse<TIn>
{
    public override string Title => $"Shuttle of {typeof(TIn).Name} -> {typeof(TOut).Name}";
    public override SpellSchool SpellSchool => SpellSchool.Shuttling;

    readonly Func<IObservable<TIn>, IObservable<TOut>> _spell;

    public ShuttleSpell(
        object who,
        Func<IObservable<TIn>, IObservable<TOut>> spell,
        Library library,
        Eris eris) : base(who, library, eris)
    {
        _spell = spell;
        InitializeLooming();
    }

    public override Dictionary<Type, bool> SatisfiedRequirements { get; } = new(1)
    {
        { typeof(TIn), false },
    };

    protected override IObservable<TOut> CreateConjuring()
    {
        var lastTIn = default(TIn);
        IObservable<TIn> ingredient = ThisAsBinding
            .GetObservableIngredient<TIn>()
            .Do(next => lastTIn = next);

        return _spell
            .Invoke(ingredient)
            .Select(matter =>
            {
                matter = matter.WithCircumstances<TOut>(lastTIn);
                ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped);
                return matter;
            });
    }
}
