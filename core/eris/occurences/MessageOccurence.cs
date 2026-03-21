using System;
using System.Collections.Generic;

namespace Rzeka
{
    public class MessageOccurence : IOccurence
    {
        public Guid Guid { get; set; }
        public Guid[] Circumstances { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public RzekaMessageType RzekaMessageType { get; set; }
        public string Message { get; set; }
        public Exception? Exception { get; set; }
    }
}