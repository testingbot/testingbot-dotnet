using System.Text.Json.Serialization;

namespace TestingBot.Api.Models;

/// <summary>
/// A build — an aggregation of tests sharing the same <c>capabilities.build</c> identifier — as
/// returned by <c>GET /v1/builds</c>. To list the tests within a build, use the builds client's
/// get-tests operation.
/// </summary>
public sealed record Build
{
    /// <summary>The unique numeric build id.</summary>
    [JsonPropertyName("id")]
    public long Id { get; init; }

    /// <summary>The user-supplied build identifier (the <c>capabilities.build</c> value).</summary>
    [JsonPropertyName("build_identifier")]
    public string? BuildIdentifier { get; init; }

    /// <summary>When the first test in the build started.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>When the build was last updated.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }
}
