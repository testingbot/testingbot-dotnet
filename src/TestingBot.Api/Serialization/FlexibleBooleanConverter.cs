using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestingBot.Api.Serialization;

/// <summary>
/// Reads booleans tolerantly from the TestingBot API, which represents truthiness as a JSON
/// boolean, an integer (<c>0</c>/<c>1</c>), or a string (<c>"true"</c>/<c>"false"</c>/<c>"1"</c>/<c>"0"</c>).
/// Always writes a canonical JSON boolean.
/// </summary>
internal sealed class FlexibleBooleanConverter : JsonConverter<bool>
{
    public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Number:
                return reader.TryGetInt64(out var number) ? number != 0 : reader.GetDouble() != 0d;
            case JsonTokenType.Null:
                return false;
            case JsonTokenType.String:
                var text = reader.GetString();
                return text?.Trim().ToLowerInvariant() switch
                {
                    "true" or "1" or "yes" or "t" => true,
                    "false" or "0" or "no" or "f" or "" or null => false,
                    _ => throw new JsonException($"Cannot convert '{text}' to a boolean."),
                };
            default:
                throw new JsonException($"Unexpected token {reader.TokenType} when reading a boolean.");
        }
    }

    public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteBooleanValue(value);
    }
}
