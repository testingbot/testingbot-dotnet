using System.Text.Json;
using System.Text.Json.Serialization;
using TestingBot.Api.Serialization;

namespace TestingBot.Api.Models;

/// <summary>
/// A screenshot thumbnail attached to a test. The API renders thumbnails as full objects on the
/// single-test endpoint and as bare URL strings on list endpoints; both deserialize into this type
/// (a bare string populates only <see cref="Url"/>).
/// </summary>
[JsonConverter(typeof(TestThumbConverter))]
public sealed record TestThumb
{
    /// <summary>The thumbnail id, when provided.</summary>
    public long? Id { get; init; }

    /// <summary>The id of the test the thumbnail belongs to, when provided.</summary>
    public long? TestCaseId { get; init; }

    /// <summary>The thumbnail filename, when provided.</summary>
    public string? Filename { get; init; }

    /// <summary>Whether the thumbnail is a custom (user-supplied) screenshot.</summary>
    public bool? Custom { get; init; }

    /// <summary>When the thumbnail was created, when provided.</summary>
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>When the thumbnail was last updated, when provided.</summary>
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>The signed URL of the thumbnail image.</summary>
    public string? Url { get; init; }
}

/// <summary>Reads a <see cref="TestThumb"/> from either a bare URL string or a full object.</summary>
internal sealed class TestThumbConverter : JsonConverter<TestThumb>
{
    public override TestThumb Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return new TestThumb { Url = reader.GetString() };
        }

        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException($"Unexpected token {reader.TokenType} when reading a test thumbnail.");
        }

        long? id = null;
        long? testCaseId = null;
        string? filename = null;
        bool? custom = null;
        DateTimeOffset? createdAt = null;
        DateTimeOffset? updatedAt = null;
        string? url = null;

        while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
        {
            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                continue;
            }

            var name = reader.GetString();
            reader.Read();
            switch (name)
            {
                case "id":
                    id = reader.TokenType == JsonTokenType.Number ? reader.GetInt64() : null;
                    break;
                case "test_case_id":
                    testCaseId = reader.TokenType == JsonTokenType.Number ? reader.GetInt64() : null;
                    break;
                case "filename":
                    filename = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    break;
                case "custom":
                    custom = ReadFlexibleBool(ref reader);
                    break;
                case "created_at":
                    createdAt = ReadDate(ref reader);
                    break;
                case "updated_at":
                    updatedAt = ReadDate(ref reader);
                    break;
                case "url":
                    url = reader.TokenType == JsonTokenType.String ? reader.GetString() : null;
                    break;
                default:
                    reader.Skip();
                    break;
            }
        }

        return new TestThumb
        {
            Id = id,
            TestCaseId = testCaseId,
            Filename = filename,
            Custom = custom,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            Url = url,
        };
    }

    public override void Write(Utf8JsonWriter writer, TestThumb value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);
        writer.WriteStartObject();
        WriteNumber(writer, "id", value.Id);
        WriteNumber(writer, "test_case_id", value.TestCaseId);
        WriteString(writer, "filename", value.Filename);
        if (value.Custom.HasValue)
        {
            writer.WriteBoolean("custom", value.Custom.Value);
        }

        WriteString(writer, "url", value.Url);
        writer.WriteEndObject();
    }

    private static bool? ReadFlexibleBool(ref Utf8JsonReader reader) => reader.TokenType switch
    {
        JsonTokenType.True => true,
        JsonTokenType.False => false,
        JsonTokenType.Number => reader.GetInt32() != 0,
        _ => null,
    };

    private static DateTimeOffset? ReadDate(ref Utf8JsonReader reader)
        => reader.TokenType == JsonTokenType.String && DateTimeOffset.TryParse(reader.GetString(), out var value)
            ? value
            : null;

    private static void WriteNumber(Utf8JsonWriter writer, string name, long? value)
    {
        if (value.HasValue)
        {
            writer.WriteNumber(name, value.Value);
        }
    }

    private static void WriteString(Utf8JsonWriter writer, string name, string? value)
    {
        if (value is not null)
        {
            writer.WriteString(name, value);
        }
    }
}
