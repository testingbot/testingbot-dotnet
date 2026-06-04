using System.Text.Json;

namespace TestingBot.Api.Serialization;

/// <summary>Small tolerant accessors for ad-hoc <see cref="JsonElement"/> payloads.</summary>
internal static class JsonElementReader
{
    public static long GetInt64OrDefault(this JsonElement element, string propertyName, long fallback = 0)
    {
        if (element.ValueKind == JsonValueKind.Object && element.TryGetProperty(propertyName, out var value))
        {
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number))
            {
                return number;
            }

            if (value.ValueKind == JsonValueKind.String && long.TryParse(value.GetString(), out var parsed))
            {
                return parsed;
            }
        }

        return fallback;
    }

    public static string? GetStringOrNull(this JsonElement element, string propertyName)
        => element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(propertyName, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;

    public static bool GetBoolOrFalse(this JsonElement element, string propertyName)
    {
        if (element.ValueKind != JsonValueKind.Object || !element.TryGetProperty(propertyName, out var value))
        {
            return false;
        }

        return value.ValueKind switch
        {
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Number => value.TryGetInt64(out var n) && n != 0,
            JsonValueKind.String => value.GetString() is "true" or "1",
            _ => false,
        };
    }
}
