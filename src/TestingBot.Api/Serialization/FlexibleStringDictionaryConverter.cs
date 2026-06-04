using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestingBot.Api.Serialization;

/// <summary>
/// Reads a <c>name → value</c> map that the API renders as a JSON object when populated, or as an
/// empty array (<c>[]</c>) when there is nothing to report (the shape used by the <c>logs</c> field).
/// Both produce a dictionary; the empty array yields an empty dictionary.
/// </summary>
internal sealed class FlexibleStringDictionaryConverter : JsonConverter<IReadOnlyDictionary<string, string>>
{
    public override IReadOnlyDictionary<string, string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return new Dictionary<string, string>();
            case JsonTokenType.StartArray:
                reader.Skip();
                return new Dictionary<string, string>();
            case JsonTokenType.StartObject:
                var map = new Dictionary<string, string>(StringComparer.Ordinal);
                while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
                {
                    if (reader.TokenType != JsonTokenType.PropertyName)
                    {
                        continue;
                    }

                    var key = reader.GetString()!;
                    reader.Read();
                    map[key] = reader.TokenType == JsonTokenType.String ? reader.GetString() ?? string.Empty : reader.GetRawValue();
                }

                return map;
            default:
                throw new JsonException($"Unexpected token {reader.TokenType} when reading a string dictionary.");
        }
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyDictionary<string, string> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        writer.WriteStartObject();
        foreach (var pair in value)
        {
            writer.WritePropertyName(pair.Key);
            writer.WriteStringValue(pair.Value);
        }

        writer.WriteEndObject();
    }
}
