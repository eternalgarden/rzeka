using System;
using System.Reactive.Linq;

namespace Rzeka;

// One-shot emit. Lifecycle: Created -> MatterOccurence(Shaped) -> Forgotten.
// Excluded from mana-availability tracking on purpose; a pluck is instantaneous,
// not a sustained source.
public sealed class PluckingSpell<TOut> : IStrandingSpell<TOut> where TOut : IMatter
{
    public Guid Guid { get; }
    public object Who { get; }
    public SpellSchool SpellSchool => SpellSchool.Plucking;
    public string Title => $"Plucking of {typeof(TOut).Name}";
    public ISpell ThisAsBase { get; }
    public IStrandingSpell<TOut> ThisAsStranding { get; }
    public IObservable<TOut> Conjuring { get; set; }
    public CollectibleDisposable Q { get; set; }
    public Type ConjuredType => typeof(TOut);
    public Library Library { get; }
    public Eris Eris { get; }

    readonly TOut _matter;

    bool _isChanneling;
    bool ISpell.HasMana
    {
        get => _isChanneling;
        set => _isChanneling = value;
    }

    bool _disposed;

    IObservable<TOut> IStrandingSpell<TOut>.CreateConjuring()
    {
        bool manual = _matter.HasCircumstances();
        return Observable
            .Return(_matter)
            .Do(m => ThisAsBase.SendMatterOccurence(m, MatterOccurenceCategory.Shaped, manual))
            .WhisperOnError(this);
    }

    public PluckingSpell(object who, TOut matter, Library library, Eris eris)
    {
        _matter = matter;

        Guid = Guid.NewGuid();
        Who = who;
        Eris = eris;
        Library = library;

        ThisAsBase = this;
        ThisAsStranding = this;

        ThisAsBase.InitializeSpellBase();
        ThisAsStranding.InitializeConjuringSpell();

        _isChanneling = true;
        Cast();
        Dispose();
    }

    public void Cast()
    {
        // Register the one-shot conjurer; subscription fires Observable.Return synchronously
        // through the Do side-effect, delivering the matter and emitting MatterOccurence.
        // Token is disposed immediately — the emission already happened.
        using IDisposable token = Library.RegisterConjurer(Conjuring);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Q?.Dispose();
        ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
    }
}
