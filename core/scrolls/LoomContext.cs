using System;

namespace Rzeka;
public class LoomContext
{
    readonly Eris _eris;
    readonly TSpell _spell;
    readonly Func<TMatter[]> _getIngredients;

    internal LoomContext(Eris eris, TSpell spell, Func<TMatter[]> getIngredients)
    {
        _eris = eris;
        _spell = spell;
        _getIngredients = getIngredients;
    }

    internal void PublishReacting(TMatter result)
    {
        _eris.PublishReactingOccurence(new ReactingOccurence
        {
            Guid = Guid.NewGuid(),
            Timestamp = DateTimeOffset.Now,
            Source = _spell,
            Triggers = _getIngredients(),
            Result = result,
        });
    }
}
