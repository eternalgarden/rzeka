using System;

namespace Rzeka
{
    public struct ReactingOccurence : IOccurence
    {
        public Guid Guid { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public TSpell Source { get; set; }
        public TMatter[] Triggers { get; set; }
        public TMatter Result { get; set; }
    }
}
