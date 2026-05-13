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
        
        var lastT = default(T1); // * attach last matter grabber
        IObservable<T1> ingredientT = ThisAsBinding
            .GetObservableIngredient<T1>()
            .Do(nextT =>
            {
                lastT = nextT;
            });

        var conjuring = _spell
            .Invoke(ingredientT)
            .Select(matter =>
            {

                /* ⭐ ---- ---- */
                
                // Automatic circumstance tracking — only if the user hasn't already set them
                // (e.g. manually via .WithCircumstances() inside an async Observable.Create wrapper)
                bool manualCircumstances = matter.HasCircumstances();
                if (!manualCircumstances)
                    matter = matter.WithCircumstances<TOut>(lastT);

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
