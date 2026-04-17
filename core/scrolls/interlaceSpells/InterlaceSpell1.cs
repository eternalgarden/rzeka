using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Rzeka;
[Serializable]
public class InterlaceSpell1<T1, TOut> : LoomingSpell<TOut>
    where TOut : TMatter
    where T1 : TMatter
{
    public override string Title => $"Interlacing of {typeof(T1).Name}";

    readonly Func<IObservable<T1>, LoomContext, IObservable<TOut>> _spell;

    public InterlaceSpell1(
        object who,
        Func<IObservable<T1>, LoomContext, IObservable<TOut>> spell,
        Library library,
        Eris eris) : base(who, library, eris)
    {
        _spell = spell;
        InitializeLooming();
    }

    public override Dictionary<Type, bool> SatisfiedRequirements { get; } = new(1)
    {
        { typeof(T1), false },
    };

    protected override IObservable<TOut> CreateConjuring()
    {
        var lastT1 = default(T1);
        IObservable<T1> ingredient1 = ThisAsBinding
            .GetObservableIngredient<T1>()
            .Do(next => lastT1 = next);

        var ctx = new LoomContext(Eris, ThisAsBase, () => new TMatter[] { lastT1 });

        return _spell
            .Invoke(ingredient1, ctx)
            .Select(matter =>
            {
                bool manualCircumstances = matter.HasCircumstances();
                if (!manualCircumstances)
                    matter = matter.WithCircumstances<TOut>(lastT1);
                ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped, manualCircumstances);
                return matter;
            });
    }
}
