using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SG.Api.Extensions
{
    public class PlayerIdConverter : JsonConverter<object>
    {
        private const int MaxPlayerIdLength = 50;
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.String)
            {
                var stringValue = reader.GetString();

                if (stringValue.Length > MaxPlayerIdLength)
                {
                    throw new JsonException("PlayerId cannot exceed 50 characters.");
                }
                if (stringValue.Contains("~"))
                {
                    throw new JsonException("PlayerId cannot contain the '~' character.");
                }

                return stringValue;
            }
            else if (reader.TokenType == JsonTokenType.Number && reader.TryGetInt32(out int intValue))
            {
                return intValue;
            }
            else
            {
                throw new JsonException("PlayerId must be a string or int.");
            }
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            if (value is int intValue)
            {
                writer.WriteNumberValue(intValue);
            }
            else if (value is string stringValue)
            {
                writer.WriteStringValue(stringValue);
            }
            else
            {
                throw new JsonException("PlayerId must be a string or int.");
            }
        }
    }
}