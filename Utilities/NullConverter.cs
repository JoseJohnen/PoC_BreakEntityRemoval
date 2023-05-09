using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Interfaz.Utilities
{
    public class NullConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return reader.GetString();
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            if (value == "null") writer.WriteNullValue();
            else writer.WriteStringValue(value);
        }
    }
}
