using System.Text.Json.Serialization;

namespace TestingBot.Api.Models;

/// <summary>A single recorded Selenium-IDE step within a Codeless test.</summary>
public sealed record CodelessStep
{
    /// <summary>The zero-based step index.</summary>
    [JsonPropertyName("test_order")]
    public int TestOrder { get; init; }

    /// <summary>The Selenium-IDE command (e.g. <c>open</c>, <c>click</c>, <c>type</c>).</summary>
    [JsonPropertyName("cmd")]
    public string? Command { get; init; }

    /// <summary>The Selenium target/locator (e.g. <c>id=foo</c>, <c>xpath=//div</c>).</summary>
    [JsonPropertyName("locator")]
    public string? Locator { get; init; }

    /// <summary>The Selenium value parameter, when the command requires one.</summary>
    [JsonPropertyName("value")]
    public string? Value { get; init; }

    /// <summary>When the step was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>When the step was last updated.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }
}
