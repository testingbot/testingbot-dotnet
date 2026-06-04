using System.Text.Json.Serialization;

namespace TestingBot.Api;

/// <summary>
/// Pagination metadata returned by TestingBot list endpoints in the <c>meta</c> envelope.
/// </summary>
public sealed record PageMeta
{
    /// <summary>The number of items skipped from the start of the full result set.</summary>
    [JsonPropertyName("offset")]
    public int Offset { get; init; }

    /// <summary>The number of items returned in this page.</summary>
    [JsonPropertyName("count")]
    public int Count { get; init; }

    /// <summary>The total number of items available across all pages.</summary>
    [JsonPropertyName("total")]
    public int Total { get; init; }

    /// <summary>Whether more items exist beyond this page, computed from <see cref="Offset"/>, <see cref="Count"/>, and <see cref="Total"/>.</summary>
    [JsonIgnore]
    public bool HasMore => Offset + Count < Total;
}
