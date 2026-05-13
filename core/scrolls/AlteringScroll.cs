using System;
using System.Collections.Generic;

namespace Rzeka;
[Serializable]
public sealed class AlteringScroll<T1> : IWeavingSpell
    where T1 : IMatter
{
    readonly IObserver<T1> _spell;
    IDisposable _subscriptionDisposable;

    public Guid Guid { get; }
    public Library Library { get; }
    public Eris Eris { get; }
    public CollectibleDisposable Q { get; set; }
    public object Who { get; }
    public ISpell ThisAsBase  { get; }
    public IBindingSpell ThisAsBinding { get; }
    public SpellSchool SpellSchool => SpellSchool.Weaving;
    public string Title => $"{Who.GetType().Name}'s Weaving of {typeof(T1).Name}";
    
    readonly Dictionary<Type, bool> _satisfiedRequirements = new(1)
    {
        { typeof(T1), false },
    };

    bool _isChanneling;

    public Dictionary<Type, bool> SatisfiedRequirements => _satisfiedRequirements;
    
    public AlteringScroll(object who, IObserver<T1> spell, Library library, Eris eris)
    {
        _spell = spell;

        Guid = Guid.NewGuid();
        Library = library;
        Eris = eris;
        Who = who;
        ThisAsBase = this;
        ThisAsBinding = this;
        
        ThisAsBase.InitializeSpellBase();
        ThisAsBinding.InitializeBindingSpell();
        
        Cast(); // ! this is weird but currently weavings should cast immediately aswell
    }

    
    //
    // ⛺ ─── Public ───────────────────────────────────────────────────
    //
    #region Public
    
    public void Cast()
    {
        try
        {
            ExecuteCast(); 
        }
        catch (Exception ex)
        {
            ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Wispd, ex);
            throw;
        }
    }

    bool ISpell.HasMana
    {
        get => _isChanneling;
        set => _isChanneling = value;
    }

    public void Dispose()
    {
        _subscriptionDisposable?.Dispose(); // TODO check this
        Q.Dispose();
        ThisAsBase.SendSpellOccurence(SpellOccurenceCategory.Forgotten);
    }
    
    #endregion // ---------------------------------- Public -------------------------

    void ExecuteCast()
    {
        // todo add this check
        // if (ThisAsBinding.IsCastable is false) throw new Exception("messed up");

        var ingredient = ThisAsBinding.GetObservableIngredient<T1>();

        if (ingredient is null) throw new Exception($"Missing ingredient of type {typeof(T1)}");
        
        _subscriptionDisposable = ingredient
            .Subscribe(_spell);
    }
}
