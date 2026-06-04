using System.Text.Json.Serialization;

namespace TestingBot.Api.Models;

/// <summary>
/// A physical mobile device in the TestingBot grid, as returned by the <c>/v1/devices</c> endpoints.
/// <see cref="Available"/> reflects whether the authenticated account can acquire the device right now.
/// </summary>
public sealed record Device
{
    /// <summary>The unique numeric device id.</summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>The marketing name (e.g. <c>iPhone 15 Pro</c>).</summary>
    [JsonPropertyName("name")]
    public string? Name { get; init; }

    /// <summary>The device model identifier.</summary>
    [JsonPropertyName("model")]
    public string? Model { get; init; }

    /// <summary>The manufacturer (e.g. <c>Apple</c>, <c>Samsung</c>).</summary>
    [JsonPropertyName("manufacturer")]
    public string? Manufacturer { get; init; }

    /// <summary>The mobile OS family (e.g. <c>iOS</c>, <c>Android</c>).</summary>
    [JsonPropertyName("platform_name")]
    public string? PlatformName { get; init; }

    /// <summary>The OS version (e.g. <c>17.4</c>, <c>14</c>).</summary>
    [JsonPropertyName("platform_version")]
    public string? PlatformVersion { get; init; }

    /// <summary>The physical display size, in inches (e.g. <c>6.1</c>).</summary>
    [JsonPropertyName("screen_size")]
    public string? ScreenSize { get; init; }

    /// <summary>The display resolution (e.g. <c>1170x2532</c>).</summary>
    [JsonPropertyName("screen_resolution")]
    public string? ScreenResolution { get; init; }

    /// <summary>Whether the device is available for the authenticated account to acquire right now.</summary>
    [JsonPropertyName("available")]
    public bool Available { get; init; }

    /// <summary>Whether the device is offered on the free trial.</summary>
    [JsonPropertyName("free_trial")]
    public bool? FreeTrial { get; init; }
}
