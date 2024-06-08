using System;
using JetBrains.Annotations;

namespace Rzeka
{
    [Serializable]
    public class SerializableMessageOccurence
    {
        public OccurenceCategory occurenceCategory => OccurenceCategory.Message;
        public Guid guid { get; set; }
        public RzekaMessageType messageType { get; set; }
        public Guid[] circumstances { get; set; }
        public long timestamp { get; set; }
        public string message { get; set; }
        [CanBeNull] public SerializableException exception { get; set; }

        public static SerializableMessageOccurence FromMessageOccurence(MessageOccurence msg)
        {
            return new SerializableMessageOccurence()
            {
                guid = msg.Guid,
                messageType = msg.RzekaMessageType,
                circumstances = msg.Circumstances,
                timestamp = msg.Timestamp.ToUnixTimeSeconds(),
                message = msg.Message,
                exception = SerializableException.FromException(msg.Exception)
            };
        }
    }
}