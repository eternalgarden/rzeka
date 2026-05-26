using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Rzeka;
[Serializable]
public class LoomingSpell2<T1, T2, TOut> : LoomingSpell<TOut>
    where TOut : IMatter
    where T1 : IMatter
    where T2 : IMatter
{
    public override string Title => $"Looming of {typeof(T1).Name} and {typeof(T2).Name} into {typeof(TOut).Name}";

    readonly Func<IObservable<T1>, IObservable<T2>, IObservable<TOut>> _spell;

    public LoomingSpell2(
        object who,
        Func<IObservable<T1>, IObservable<T2>, IObservable<TOut>> spell,
        Library library,
        Eris eris) : base(who, library, eris)
    {
        _spell = spell;
        InitializeLooming();
    }

    public override Dictionary<Type, bool> SatisfiedRequirements { get; } = new(2)
    {
        { typeof(T1), false },
        { typeof(T2), false },
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

        return _spell
            .Invoke(ingredient1, ingredient2)
            .ObserveOn(Eris.MainThread)
            .Select(matter =>
            {
                if (!ingredient1Subscribed || !ingredient2Subscribed)
                    Eris.PublishMessage(new MessageOccurence
                    {
                        Guid = Guid.NewGuid(),
                        Timestamp = DateTimeOffset.Now,
                        RzekaMessageType = RzekaMessageType.Horror,
                        Message = $"Loom output {typeof(TOut).Name} fired without the '{(!ingredient1Subscribed ? typeof(T1).Name : typeof(T2).Name)}' ingredient being subscribed to. A declared input was never wired into the observable chain.",
                    });

                bool manualCircumstances = matter.HasCircumstances();
                if (!manualCircumstances)
                    matter = matter.WithCircumstances<TOut>(
                        new IMatter?[] { lastT1, lastT2 }.Where(x => x is not null).Cast<IMatter>().ToArray()
                    );
                ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped, manualCircumstances);
                return matter;
            })
            .WhisperOnError(this);
    }
}
