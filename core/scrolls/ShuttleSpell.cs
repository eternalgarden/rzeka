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
        bool ingredientSubscribed = false;
        var lastTIn = default(TIn);
        IObservable<TIn> ingredient = Observable.Create<TIn>(observer =>
        {
            ingredientSubscribed = true;
            return ThisAsBinding
                .GetObservableIngredient<TIn>()
                .Do(next => lastTIn = next)
                .Subscribe(observer);
        });

        return _spell
            .Invoke(ingredient)
            .Select(matter =>
            {
                // Mirror LoomingSpell1: only auto-stamp [request] when the responder hasn't
                // stamped manually. Required for the multi-context Shuttle pattern, where the
                // responder uses Scry<T>() inside the lambda and stamps [req, scryedA, ...]
                // on the response themselves.
                if (!ingredientSubscribed)
                    Eris.PublishMessage(new MessageOccurence
                    {
                        Guid = Guid.NewGuid(),
                        Timestamp = DateTimeOffset.Now,
                        RzekaMessageType = RzekaMessageType.Horror,
                        Message = $"Shuttle response {typeof(TOut).Name} fired without the '{typeof(TIn).Name}' request observable being subscribed to. The lambda is not chaining from the request observable.",
                    });

                bool manualCircumstances = matter.HasCircumstances();
                if (!manualCircumstances && lastTIn is not null)
                    matter = matter.WithCircumstances<TOut>(lastTIn);

                ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped, manualCircumstances);
                return matter;
            })
            .WhisperOnError(this);
    }
}
