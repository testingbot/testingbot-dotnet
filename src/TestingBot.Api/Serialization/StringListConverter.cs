using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestingBot.Api.Serialization;

/// <summary>
/// Reads a list of strings that the API renders either as a plain string array (e.g.
/// <c>["smoke","regression"]</c>) or as an array of objects carrying a <c>name</c> property
/// (the shape used by the test-list <c>since</c> branch). Objects are reduced to their name.
/// </summary>
internal sealed class StringListConverter : JsonConverter<IReadOnlyList<string>>
{
    public override IReadOnlyList<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return [];
        }

        if (reader.TokenType != JsonTokenType.StartArray)
        {
            throw new JsonException($"Unexpected token {reader.TokenType} when reading a string list.");
        }

        var list = new List<string>();
        while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    var value = reader.GetString();
                    if (value is not null)
                    {
                        list.Add(value);
                    }

                    break;
                case JsonTokenType.StartObject:
                    var name = ReadNameFromObject(ref reader);
                    if (name is not null)
                    {
                        list.Add(name);
                    }

                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return list;
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<string> value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        writer.WriteStartArray();
        foreach (var item in value)
        {
            writer.WriteStringValue(item);
        }

        writer.WriteEndArray();
    }

    private static string? ReadNameFromObject(ref Utf8JsonReader reader)
    {
        string? name = null;
        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();
                if (string.Equals(propertyName, "name", StringComparison.OrdinalIgnoreCase) && reader.TokenType == JsonTokenType.String)
                {
                    name = reader.GetString();
                }
                else
                {
                    reader.Skip();
                }
            }
        }

        return name;
    }
}
