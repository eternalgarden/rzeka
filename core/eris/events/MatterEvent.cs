using System;
using System.Text.Json.Serialization;
using Rzeka.Internal;

namespace Rzeka;
public enum MatterEventType
{
    Shaped,
    Received,
    //Exception,
    //Finished
}

// TODO RENAME TO MATTERREALMEVENT
[Serializable]
[JsonConverter(typeof(RealmEventJsonConverter))]
public class MatterEvent : RealmEvent
{
    public readonly ISpell Scroll;
    public readonly IMatter Matter;
    public readonly MatterEventType EventType;
    
    public MatterEvent(IMatter matter, ISpell scroll, MatterEventType eventType) : base()
    {
        Matter = matter;
        Scroll = scroll;
        EventType = eventType;
    }
}
