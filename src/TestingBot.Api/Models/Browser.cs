using System.Text.Json.Serialization;

namespace TestingBot.Api.Models;

/// <summary>
/// A browser environment available on the TestingBot grid, as returned by
/// <c>GET /v1/browsers</c>. Mobile environments additionally populate
/// <see cref="DeviceName"/> and <see cref="PlatformName"/>.
/// </summary>
public sealed record Browser
{
    /// <summary>The value to send as the <c>browserName</c> capability in WebDriver.</summary>
    [JsonPropertyName("selenium_name")]
    public string? SeleniumName { get; init; }

    /// <summary>Human-readable browser identifier (e.g. <c>firefox</c>, <c>iexplore</c>).</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The operating system (e.g. <c>WINDOWS</c>, <c>MAC</c>, <c>LINUX</c>).</summary>
    [JsonPropertyName("platform")]
    public string? Platform { get; init; }

    /// <summary>The unique TestingBot browser id used when attaching browsers to Codeless tests.</summary>
    [JsonPropertyName("browser_id")]
    public int BrowserId { get; init; }

    /// <summary>The browser version (major version, or empty for versionless environments).</summary>
    [JsonPropertyName("version")]
    public string? Version { get; init; }

    /// <summary>The full version string (e.g. <c>121.0.6167.184</c>), when available.</summary>
    [JsonPropertyName("long_version")]
    public string? LongVersion { get; init; }

    /// <summary>The device name, for mobile environments.</summary>
    [JsonPropertyName("deviceName")]
    public string? DeviceName { get; init; }

    /// <summary>The platform name (e.g. <c>iOS</c>, <c>Android</c>), for mobile environments.</summary>
    [JsonPropertyName("platformName")]
    public string? PlatformName { get; init; }
}
