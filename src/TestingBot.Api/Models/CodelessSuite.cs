using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TestingBot.Api.Models;

/// <summary>A Codeless suite — a named grouping of Codeless tests, as returned by <c>/v1/labsuites</c>.</summary>
public sealed record CodelessSuite
{
    /// <summary>The unique numeric suite id.</summary>
    [JsonPropertyName("id")]
    public long Id { get; init; }

    /// <summary>The suite name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>Whether scheduled runs are active.</summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; init; }

    /// <summary>The cron expression for scheduled runs.</summary>
    [JsonPropertyName("cron")]
    public string? Cron { get; init; }

    /// <summary>The number of Codeless tests attached to the suite.</summary>
    [JsonPropertyName("test_count")]
    public int? TestCount { get; init; }

    /// <summary>When the suite was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>When the suite was last updated.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>The most recent run timestamp.</summary>
    [JsonPropertyName("last_run")]
    public DateTimeOffset? LastRun { get; init; }

    /// <summary>The configured alert destinations.</summary>
    [JsonPropertyName("alerts")]
    public IReadOnlyList<CodelessAlert>? Alerts { get; init; }

    /// <summary>The browsers the suite is configured to run on (populated by the list endpoint).</summary>
    [JsonPropertyName("browsers")]
    public IReadOnlyList<Browser>? Browsers { get; init; }
}
