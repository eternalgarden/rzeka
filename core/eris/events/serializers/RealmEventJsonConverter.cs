using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Rzeka.Internal;
/*

* BE MOST CAREFUL ABOUT CHANGES HERE

TODO Write an automated test that would make sure an example scroll and matter event is serialized into an expected format

*/
internal class RealmEventJsonConverter : JsonConverter<RealmEvent>
{
    public override void Write(Utf8JsonWriter writer, RealmEvent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Category", value.GetType().Name);

        switch (value)
        {
            case ScrollEvent scrollEvent:
                /* ⭐ ---- ---- */

                writer.WriteString("Type", scrollEvent.EventType.ToString());
                WriteCommonRealmEventProperties(writer, value);
                writer.WritePropertyName("Scroll");
                JsonSerializer.Serialize(writer, scrollEvent.Scroll, scrollEvent.Scroll?.GetType() ?? typeof(ISpell), options);
                break;

                /* ---- ---- 🌠 */

            case MatterEvent matterEvent:
                /* ⭐ ---- ---- */

                writer.WriteString("Type", matterEvent.EventType.ToString());
                WriteCommonRealmEventProperties(writer, value);
                writer.WritePropertyName("Matter");
                JsonSerializer.Serialize(writer, matterEvent.Matter, matterEvent.Matter?.GetType() ?? typeof(IMatter), options);
                writer.WritePropertyName("Scroll");
                JsonSerializer.Serialize(writer, matterEvent.Scroll, matterEvent.Scroll?.GetType() ?? typeof(ISpell), options);
                break;

                /* ---- ---- 🌠 */

            default:
                throw new ArgumentException();
        }

        writer.WriteEndObject();
    }

    static void WriteCommonRealmEventProperties(Utf8JsonWriter writer, RealmEvent realmEvent)
    {
        writer.WriteString("Guid", realmEvent.Guid);
        writer.WriteString("Timestamp", realmEvent.Timestamp);
    }

    public override RealmEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotImplementedException("This won't be implemented, can't be Read.");
    }
}
