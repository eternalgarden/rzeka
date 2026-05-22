using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Rzeka;
[Serializable]
public class LoomingSpell3<T1, T2, T3, TOut> : LoomingSpell<TOut>
    where TOut : IMatter
    where T1 : IMatter
    where T2 : IMatter
    where T3 : IMatter
{
    public override string Title => $"Looming of {typeof(T1).Name}, {typeof(T2).Name} and {typeof(T3).Name} into {typeof(TOut).Name}";

    readonly Func<IObservable<T1>, IObservable<T2>, IObservable<T3>, IObservable<TOut>> _spell;

    public LoomingSpell3(
        object who,
        Func<IObservable<T1>, IObservable<T2>, IObservable<T3>, IObservable<TOut>> spell,
        Library library,
        Eris eris) : base(who, library, eris)
    {
        _spell = spell;
        InitializeLooming();
    }

    // TODO replace with an error that informs the user the same type can't be used twice
    public override Dictionary<Type, bool> SatisfiedRequirements { get; } = new(3)
    {
        { typeof(T1), false },
        { typeof(T2), false },
        { typeof(T3), false },
    };

    protected override IObservable<TOut> CreateConjuring()
    {
        bool ingredient1Subscribed = false;
        var lastT1 = default(T1);
        IObservable<T1> ingredient1 = Observable.Create<T1>(observer =>
        {
            ingredient1Subscribed = true;
            return ThisAsBinding
                .GetObservableIngredient<T1>()
                .Do(next => lastT1 = next)
                .Subscribe(observer);
        });

        bool ingredient2Subscribed = false;
        var lastT2 = default(T2);
        IObservable<T2> ingredient2 = Observable.Create<T2>(observer =>
        {
            ingredient2Subscribed = true;
            return ThisAsBinding
                .GetObservableIngredient<T2>()
                .Do(next => lastT2 = next)
                .Subscribe(observer);
        });

        bool ingredient3Subscribed = false;
        var lastT3 = default(T3);
        IObservable<T3> ingredient3 = Observable.Create<T3>(observer =>
        {
            ingredient3Subscribed = true;
            return ThisAsBinding
                .GetObservableIngredient<T3>()
                .Do(next => lastT3 = next)
                .Subscribe(observer);
        });

        return _spell
            .Invoke(ingredient1, ingredient2, ingredient3)
            .Select(matter =>
            {
                if (!ingredient1Subscribed || !ingredient2Subscribed || !ingredient3Subscribed)
                    Eris.PublishMessage(new MessageOccurence
                    {
                        Guid = Guid.NewGuid(),
                        Timestamp = DateTimeOffset.Now,
                        RzekaMessageType = RzekaMessageType.Horror,
                        Message = $"Loom output {typeof(TOut).Name} fired without the '{(!ingredient1Subscribed ? typeof(T1).Name : !ingredient2Subscribed ? typeof(T2).Name : typeof(T3).Name)}' ingredient being subscribed to. A declared input was never wired into the observable chain.",
                    });

                bool manualCircumstances = matter.HasCircumstances();
                if (!manualCircumstances)
                    matter = matter.WithCircumstances<TOut>(
                        new IMatter?[] { lastT1, lastT2, lastT3 }.Where(x => x is not null).Cast<IMatter>().ToArray()
                    );
                ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped, manualCircumstances);
                return matter;
            })
            .WhisperOnError(this);
    }
}
