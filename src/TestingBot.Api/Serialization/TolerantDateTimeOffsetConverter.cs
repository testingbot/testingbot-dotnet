using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestingBot.Api.Serialization;

/// <summary>
/// Reads timestamps tolerantly from the TestingBot API, which returns ISO-8601 strings for most
/// fields but occasionally a Unix epoch (seconds, or milliseconds for large values). Empty strings
/// and <c>null</c> map to <see langword="null"/> for nullable targets. Always writes ISO-8601 (UTC).
/// </summary>
internal sealed class TolerantDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
{
    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return default;
            case JsonTokenType.Number:
                return FromUnix(reader.GetDouble());
            case JsonTokenType.String:
                var text = reader.GetString();
                if (string.IsNullOrWhiteSpace(text))
                {
                    return default;
                }

                if (DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var parsed))
                {
                    return parsed;
                }

                if (double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out var epoch))
                {
                    return FromUnix(epoch);
                }

                throw new JsonException($"Cannot convert '{text}' to a DateTimeOffset.");
            default:
                throw new JsonException($"Unexpected token {reader.TokenType} when reading a DateTimeOffset.");
        }
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        writer.WriteStringValue(value.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture));
    }

    private static DateTimeOffset FromUnix(double value)
    {
        // Values beyond ~year 2286 in seconds are almost certainly milliseconds.
        const double MillisecondThreshold = 1e12;
        return value >= MillisecondThreshold
            ? DateTimeOffset.FromUnixTimeMilliseconds((long)value)
            : DateTimeOffset.FromUnixTimeSeconds((long)value);
    }
}
