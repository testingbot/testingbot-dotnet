using System.Collections.Generic;
using System.Text.Json.Serialization;
using TestingBot.Api.Serialization;

namespace TestingBot.Api.Models;

/// <summary>
/// A test session, as returned by the <c>/v1/tests</c> endpoints. The API populates a slightly
/// different subset of fields on list versus single-test responses; unset fields are
/// <see langword="null"/>. The server-rendered HTML <c>steps</c> field is intentionally not modeled.
/// </summary>
public sealed record TestCase
{
    /// <summary>The unique numeric test id.</summary>
    [JsonPropertyName("id")]
    public long Id { get; init; }

    /// <summary>The Selenium / WebDriver session id (a UUID).</summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; init; }

    /// <summary>The human-readable test name.</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The lifecycle state (e.g. <c>RUNNING</c>, <c>COMPLETE</c>, <c>TIMEOUT</c>).</summary>
    [JsonPropertyName("state")]
    public string? State { get; init; }

    /// <summary>Whether the test passed.</summary>
    [JsonPropertyName("success")]
    public bool? Success { get; init; }

    /// <summary>The numeric status code (<c>0</c> = fail, <c>1</c> = pass, <c>2</c> = unknown).</summary>
    [JsonPropertyName("status_id")]
    public int? StatusId { get; init; }

    /// <summary>Whether the result is explicitly unknown (status code 2).</summary>
    [JsonPropertyName("unknown")]
    public bool? Unknown { get; init; }

    /// <summary>The failure reason or arbitrary status text.</summary>
    [JsonPropertyName("status_message")]
    public string? StatusMessage { get; init; }

    /// <summary>When the test session started.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>When the session ended; <see langword="null"/> while running.</summary>
    [JsonPropertyName("completed_at")]
    public DateTimeOffset? CompletedAt { get; init; }

    /// <summary>The total run time, in seconds.</summary>
    [JsonPropertyName("duration")]
    public int? Duration { get; init; }

    /// <summary>The browser identifier (e.g. <c>firefox</c>), or a combined name for mobile sessions.</summary>
    [JsonPropertyName("browser")]
    public string? Browser { get; init; }

    /// <summary>The browser version.</summary>
    [JsonPropertyName("browser_version")]
    public string? BrowserVersion { get; init; }

    /// <summary>The operating system (e.g. <c>WINDOWS</c>, <c>MAC</c>).</summary>
    [JsonPropertyName("os")]
    public string? Os { get; init; }

    /// <summary>The mobile device name, when the session ran on a physical device.</summary>
    [JsonPropertyName("device_name")]
    public string? DeviceName { get; init; }

    /// <summary>The mobile platform name, when the session ran on a physical device.</summary>
    [JsonPropertyName("platform_name")]
    public string? PlatformName { get; init; }

    /// <summary>The driver type (e.g. <c>WEBDRIVER</c>, <c>APPIUM</c>).</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>The build identifier this test is associated with.</summary>
    [JsonPropertyName("build")]
    public string? Build { get; init; }

    /// <summary>Tag/group names attached to the test.</summary>
    [JsonPropertyName("groups")]
    [JsonConverter(typeof(StringListConverter))]
    public IReadOnlyList<string>? Groups { get; init; }

    /// <summary>The signed URL of the recorded video, or <see langword="null"/> when video was disabled.</summary>
    [JsonPropertyName("video")]
    [JsonConverter(typeof(FalseAsNullStringConverter))]
    public string? Video { get; init; }

    /// <summary>Screenshot thumbnails for the test.</summary>
    [JsonPropertyName("thumbs")]
    public IReadOnlyList<TestThumb>? Thumbs { get; init; }

    /// <summary>A map of log name to signed URL (e.g. <c>selenium</c>, <c>browser</c>).</summary>
    [JsonPropertyName("logs")]
    [JsonConverter(typeof(FlexibleStringDictionaryConverter))]
    public IReadOnlyDictionary<string, string>? Logs { get; init; }

    /// <summary>Whether assets (video, logs, screenshots) have finished processing.</summary>
    [JsonPropertyName("assets_available")]
    public bool? AssetsAvailable { get; init; }

    /// <summary>Arbitrary metadata string set via <c>test[extra]</c>.</summary>
    [JsonPropertyName("extra")]
    public string? Extra { get; init; }

    /// <summary>Completion percentage while a Codeless test is running, when applicable.</summary>
    [JsonPropertyName("running")]
    public int? Running { get; init; }
}
