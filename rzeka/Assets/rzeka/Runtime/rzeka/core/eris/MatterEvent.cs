using System;
using Newtonsoft.Json;
using Rzeka.Internal;
using UnityEngine;

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
        public readonly TScrollBase Scroll;
        public readonly TMatter Matter;
        public readonly MatterEventType EventType;
        
        public MatterEvent(TMatter matter, TScrollBase scroll, MatterEventType eventType) : base()
        {
            Matter = matter;
            Scroll = scroll;
            EventType = eventType;
        }
    }
}