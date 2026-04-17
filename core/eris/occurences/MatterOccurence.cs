using System;

namespace Rzeka;
public struct MatterOccurence : IOccurence
{
    public Guid Guid { get; set; }
    public DateTimeOffset Timestamp { get; set; }
    public TSpell Source { get; set; }
    public MatterOccurenceCategory MatterOccurenceCategory { get; set; }
    public TMatter Matter { get; set; }
    public bool ManualCircumstances { get; set; }
}
