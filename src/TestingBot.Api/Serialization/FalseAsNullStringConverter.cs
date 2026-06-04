using System.Buffers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestingBot.Api.Serialization;

/// <summary>
/// Reads a string that the API renders as <see langword="false"/> when absent (notably the
/// <c>video</c> field, which is either a signed URL or the boolean <c>false</c>). Booleans and
/// <c>null</c> map to <see langword="null"/>.
/// </summary>
internal sealed class FalseAsNullStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString(),
            JsonTokenType.True or JsonTokenType.False or JsonTokenType.Null => null,
            JsonTokenType.Number => reader.GetRawValue(),
            _ => throw new JsonException($"Unexpected token {reader.TokenType} when reading a nullable string."),
        };

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        if (value is null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }
}

internal static class Utf8JsonReaderExtensions
{
    public static string GetRawValue(this scoped ref Utf8JsonReader reader)
    {
        if (reader.HasValueSequence)
        {
            return System.Text.Encoding.UTF8.GetString(reader.ValueSequence.ToArray());
        }

        return System.Text.Encoding.UTF8.GetString(reader.ValueSpan);
    }
}
