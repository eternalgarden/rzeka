using System;
using System.Text.Json.Serialization;
using Rzeka.Internal;

namespace Rzeka
{
    [Flags]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ScrollEventType
    {
        New         = 1,
        Known         = 1 << 1,
        Cast        = 1 << 2,
        Blocked     = 1 << 3,
        Requested   = 1 << 4,
        Forgotten   = 1 << 5,
    }
    
    [Serializable]
    [JsonConverter(typeof(RealmEventJsonConverter))]
    public class ScrollEvent : RealmEvent
    {
        public readonly TSpell Scroll;
        public readonly ScrollEventType EventType;
        
        public ScrollEvent(TSpell scroll, ScrollEventType eventType) : base()
        {
            Scroll = scroll;
            EventType = eventType;
        }
    }
}