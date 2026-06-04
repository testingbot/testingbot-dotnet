using System.Text.Json.Serialization;

namespace TestingBot.Api.Models;

/// <summary>
/// An app binary uploaded to TestingBot storage, as returned by the <c>/v1/storage</c> endpoints.
/// Upload responses populate only <see cref="AppUrl"/>; list and get responses populate the rest.
/// </summary>
public sealed record StorageFile
{
    /// <summary>The unique numeric storage object id.</summary>
    [JsonPropertyName("id")]
    public long? Id { get; init; }

    /// <summary>The TestingBot storage URL (<c>tb://&lt;appkey&gt;</c>) to reference this app as a capability.</summary>
    [JsonPropertyName("app_url")]
    public string? AppUrl { get; init; }

    /// <summary>A signed HTTPS URL to download the binary directly.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>The original uploaded filename.</summary>
    [JsonPropertyName("filename")]
    public string? Filename { get; init; }

    /// <summary>The detected app type (<c>apk</c>, <c>ipa</c>, <c>zip</c>).</summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }

    /// <summary>The version string extracted from the binary.</summary>
    [JsonPropertyName("version")]
    public string? Version { get; init; }

    /// <summary>The minimum OS version required by the app.</summary>
    [JsonPropertyName("min_device_version")]
    public string? MinDeviceVersion { get; init; }

    /// <summary>A signed URL to the app icon.</summary>
    [JsonPropertyName("thumb")]
    public string? Thumb { get; init; }

    /// <summary>The processing state (<c>PROCESSING</c>, <c>READY</c>).</summary>
    [JsonPropertyName("state")]
    public string? State { get; init; }

    /// <summary>Whether this is an iOS simulator-only build.</summary>
    [JsonPropertyName("sim_only")]
    public bool? SimOnly { get; init; }

    /// <summary>The upload timestamp.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>The app key (the <c>tb://</c> prefix stripped from <see cref="AppUrl"/>).</summary>
    [JsonIgnore]
    public string? AppKey => AppUrl is null
        ? null
        : AppUrl.StartsWith("tb://", StringComparison.OrdinalIgnoreCase) ? AppUrl[5..] : AppUrl;
}
