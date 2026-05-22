using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Rzeka;
[Serializable]
public class LoomingSpell1<T1, TOut> : LoomingSpell<TOut>
    where TOut : IMatter
    where T1 : IMatter
{
    public override string Title => $"Looming of {typeof(T1).Name}";
    
    readonly Func<IObservable<T1>, IObservable<TOut>> _spell;

    public LoomingSpell1(object who,
        Func<IObservable<T1>, IObservable<TOut>> spell,
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
        /* ⭐ ---- ---- */

        bool ingredientSubscribed = false;
        var lastT = default(T1);
        IObservable<T1> ingredientT = Observable.Create<T1>(observer =>
        {
            ingredientSubscribed = true;
            return ThisAsBinding
                .GetObservableIngredient<T1>()
                .Do(nextT => lastT = nextT)
                .Subscribe(observer);
        });

        var conjuring = _spell
            .Invoke(ingredientT)
            .Select(matter =>
            {

                /* ⭐ ---- ---- */

                if (!ingredientSubscribed)
                    Eris.PublishMessage(new MessageOccurence
                    {
                        Guid = Guid.NewGuid(),
                        Timestamp = DateTimeOffset.Now,
                        RzekaMessageType = RzekaMessageType.Horror,
                        Message = $"Loom output {typeof(TOut).Name} fired without the '{typeof(T1).Name}' ingredient being subscribed to. The lambda is not chaining from the provided input observable — use 'events.Select(_ => ...)' not 'Observable.Return(...)'.",
                    });

                // Automatic circumstance tracking — only if the user hasn't already set them
                // (e.g. manually via .WithCircumstances() inside an async Observable.Create wrapper)
                bool manualCircumstances = matter.HasCircumstances();
                if (!manualCircumstances)
                    matter = matter.WithCircumstances<TOut>(
                        lastT is not null ? new IMatter[] { lastT } : Array.Empty<IMatter>()
                    );

                ThisAsBase.SendMatterOccurence(matter, MatterOccurenceCategory.Shaped, manualCircumstances);

                return matter;

                /* ---- ---- 🌠 */

            })
            .WhisperOnError(this);
            // nd
            // TODO this will be deleted since Library will handle that
            // .Multicast(new ReplaySubject<TOut>(bufferSize: 1)) // ? provide alternatives
            // .RefCount();

        return conjuring;

        /* ---- ---- 🌠 */
    }
}
