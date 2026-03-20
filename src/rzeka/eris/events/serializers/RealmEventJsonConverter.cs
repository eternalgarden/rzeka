using System;
using Newtonsoft.Json;

namespace Rzeka.Internal
{
    /*

    * BE MOST CAREFUL ABOUT CHANGES HERE
    
    TODO Write an automated test that would make sure an example scroll and matter event is serialized into an expected format

    */
    internal class RealmEventJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("Category");
            writer.WriteValue(value.GetType().Name);

            if (value is not RealmEvent realmEvent) throw new ArgumentNullException(nameof(realmEvent));

            switch (realmEvent)
            {
                case ScrollEvent scrollEvent:

                    /* ⭐ ---- ---- */
                    
                    writer.WritePropertyName("Type");
                    writer.WriteValue(scrollEvent.EventType.ToString()); // ScrollEventType

                    WriteCommonRealmEventProperties(writer, realmEvent);

                    writer.WritePropertyName("Scroll");
                    serializer.Serialize(writer, scrollEvent.Scroll);
                    break;
                    
                    /* ---- ---- 🌠 */

                case MatterEvent matterEvent:

                    /* ⭐ ---- ---- */
                    
                    writer.WritePropertyName("Type");
                    writer.WriteValue(matterEvent.EventType.ToString()); // MatterEventType

                    WriteCommonRealmEventProperties(writer, realmEvent);

                    writer.WritePropertyName("Matter");
                    serializer.Serialize(writer, matterEvent.Matter);
                    writer.WritePropertyName("Scroll"); // * scroll that this matter originates from
                    serializer.Serialize(writer, matterEvent.Scroll);
                    break;
                    
                    /* ---- ---- 🌠 */

                default:
                    throw new ArgumentException();
            }

            writer.WriteEndObject();
        }

        static void WriteCommonRealmEventProperties(JsonWriter writer, RealmEvent realmEvent)
        {
            writer.WritePropertyName("Guid");
            writer.WriteValue(realmEvent.Guid);
            writer.WritePropertyName("Timestamp");
            writer.WriteValue(realmEvent.Timestamp);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException("This won't be implemented, can't be Read.");
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(RealmEvent) || objectType == typeof(MatterEvent) || objectType == typeof(ScrollEvent);
        }

        public override bool CanRead => false;
    }
}
