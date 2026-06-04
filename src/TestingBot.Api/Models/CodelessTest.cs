using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TestingBot.Api.Models;

/// <summary>A configured alert destination on a Codeless test.</summary>
public sealed record CodelessAlert
{
    /// <summary>The alert channel (<c>EMAIL</c>, <c>API</c>, <c>SMS</c>).</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>The destination value: email address, callback URL, or phone number.</summary>
    [JsonPropertyName("value")]
    public string? Value { get; init; }

    /// <summary>When to send (<c>IMMEDIATELY</c>, <c>DAILY</c>).</summary>
    [JsonPropertyName("level")]
    public string? Level { get; init; }
}

/// <summary>A Codeless (recorded, no-code) test, as returned by the <c>/v1/lab</c> endpoints.</summary>
public sealed record CodelessTest
{
    /// <summary>The unique numeric Codeless test id.</summary>
    [JsonPropertyName("id")]
    public long Id { get; init; }

    /// <summary>The test name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The target URL the test runs against.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>Whether scheduled runs are active.</summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; init; }

    /// <summary>The cron expression for scheduled runs; <see langword="null"/> when unscheduled.</summary>
    [JsonPropertyName("cron")]
    public string? Cron { get; init; }

    /// <summary>When the test was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>When the test was last updated.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>The most recent run timestamp.</summary>
    [JsonPropertyName("last_run")]
    public DateTimeOffset? LastRun { get; init; }

    /// <summary>The configured alert destinations.</summary>
    [JsonPropertyName("alerts")]
    public IReadOnlyList<CodelessAlert>? Alerts { get; init; }

    /// <summary>The browsers this test is configured to run on.</summary>
    [JsonPropertyName("browsers")]
    public IReadOnlyList<Browser>? Browsers { get; init; }
}
