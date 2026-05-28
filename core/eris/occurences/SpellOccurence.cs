using System;

namespace Rzeka;
public struct SpellOccurence : IOccurence
{
    public Guid Guid { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public ISpell Source { get; set; }
    public SpellOccurenceCategory SpellOccurenceCategory { get; set; }
}
