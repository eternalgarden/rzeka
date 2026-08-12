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
        // Whispered at most once: the check below runs per emission, and a lambda rooted at
        // a repeating generator would otherwise flood Eris with the same Horror forever.
        bool unchainedHorrorWhispered = false;
        IObservable<TIn> ingredient = Observable.Create<TIn>(observer =>
        {
            ingredientSubscribed = true;
            return ThisAsBinding
                .GetObservableIngredient<TIn>()
                .Subscribe(observer);
        });

        return _spell
            .Invoke(ingredient)
            .ObserveOn(Eris.MainThread)
            .Select(matter =>
            {
                if (!ingredientSubscribed && !unchainedHorrorWhispered)
                {
                    unchainedHorrorWhispered = true;
                    Eris.PublishMessage(new MessageOccurence
                    {
                        Guid = Guid.NewGuid(),
                        Timestamp = DateTimeOffset.Now,
                        RzekaMessageType = RzekaMessageType.Horror,
                        Message = $"Shuttle response {typeof(TOut).Name} fired without the '{typeof(TIn).Name}' request observable being subscribed to. The lambda is not chaining from the request observable. In {Title} (owned by {Who}).",
                    });
                }

                // A response is, by definition, caused by its request. Because
                // TOut : IResponse<TIn>, the request rides on the response itself
                // (matter.Request), so we read it straight from there - correct even
                // across async boundaries, with no reliance on a captured "last request"
                // that would race when several requests are in flight. Any extra context
                // the responder Scry'd in and stamped by hand is preserved: we union it
                // after the request, and since matter equality is by Guid, a hand-stamped
                // request is deduped rather than doubled.
                bool manualCircumstances = matter.HasCircumstances();
                if (matter.Request is not null)
                {
                    var circumstances = new List<IMatter>(matter.Circumstances.Count + 1)
                    {
                        matter.Request,
                    };
                    foreach (IMatter existing in matter.Circumstances)
                        if (!existing.Equals(matter.Request))
                            circumstances.Add(existing);

                    // A hand-stamped request is redundant with what we attach anyway, so
                    // it must not report as manual. 
                    manualCircumstances = circumstances.Count > 1;
                    matter = matter.WithCircumstances<TOut>(circumstances.ToArray());
                }
                else
                    Eris.PublishMessage(new MessageOccurence
                    {
                        Guid = Guid.NewGuid(),
                        Timestamp = DateTimeOffset.Now,
                        RzekaMessageType = RzekaMessageType.Horror,
                        Message = $"Shuttle response {typeof(TOut).Name} was constructed with a null '{typeof(TIn).Name}' request. Without the request reference its causality cannot be recorded, so it will break all your Ask's on {Title} (owned by {Who}).",
                    });

                ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped, manualCircumstances);
                return matter;
            })
            .WhisperOnError(this);
    }
}
