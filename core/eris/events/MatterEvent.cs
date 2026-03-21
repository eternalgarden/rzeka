using System;
using Newtonsoft.Json;
using Rzeka.Internal;

namespace Rzeka
{
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
        public readonly TSpell Scroll;
        public readonly TMatter Matter;
        public readonly MatterEventType EventType;
        
        public MatterEvent(TMatter matter, TSpell scroll, MatterEventType eventType) : base()
        {
            Matter = matter;
            Scroll = scroll;
            EventType = eventType;
        }
    }
}