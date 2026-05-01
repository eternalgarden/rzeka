using System;

namespace Rzeka;
public struct MatterOccurence : IOccurence
{
    public Guid Guid { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public ISpell Source { get; set; }
    public MatterOccurenceCategory MatterOccurenceCategory { get; set; }
    public IMatter Matter { get; set; }
    public bool ManualCircumstances { get; set; }
}
