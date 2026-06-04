using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TestingBot.Api.Models;

/// <summary>A single per-browser screenshot within a batch.</summary>
public sealed record ScreenshotImage
{
    /// <summary>The screenshot id.</summary>
    [JsonPropertyName("id")]
    public long Id { get; init; }

    /// <summary>The signed URL of the full-size image.</summary>
    [JsonPropertyName("image_url")]
    public string? ImageUrl { get; init; }

    /// <summary>The signed URL of the thumbnail.</summary>
    [JsonPropertyName("thumb_url")]
    public string? ThumbUrl { get; init; }

    /// <summary>The per-image processing state.</summary>
    [JsonPropertyName("state")]
    public string? State { get; init; }

    /// <summary>The originating session id, when applicable.</summary>
    [JsonPropertyName("session_id")]
    public string? SessionId { get; init; }

    /// <summary>The operating system the screenshot was rendered on.</summary>
    [JsonPropertyName("os")]
    public string? Os { get; init; }

    /// <summary>The browser name.</summary>
    [JsonPropertyName("browser_name")]
    public string? BrowserName { get; init; }

    /// <summary>The browser version.</summary>
    [JsonPropertyName("browser_version")]
    public string? BrowserVersion { get; init; }

    /// <summary>The browser id.</summary>
    [JsonPropertyName("browser_id")]
    public int? BrowserId { get; init; }

    /// <summary>The device name, for mobile renders.</summary>
    [JsonPropertyName("device_name")]
    public string? DeviceName { get; init; }

    /// <summary>The platform name, for mobile renders.</summary>
    [JsonPropertyName("platform_name")]
    public string? PlatformName { get; init; }

    /// <summary>When the screenshot was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }
}

/// <summary>
/// A cross-browser screenshot batch, as returned by the <c>/v1/screenshots</c> endpoints. The list
/// endpoint populates a lightweight subset; the create and detail endpoints populate more fields.
/// </summary>
public sealed record Screenshot
{
    /// <summary>The unique numeric batch id.</summary>
    [JsonPropertyName("id")]
    public long Id { get; init; }

    /// <summary>The page URL that was captured.</summary>
    [JsonPropertyName("url")]
    public string? Url { get; init; }

    /// <summary>The browser viewport resolution (e.g. <c>1920x1080</c>).</summary>
    [JsonPropertyName("resolution")]
    public string? Resolution { get; init; }

    /// <summary>The seconds the browser waited before snapping.</summary>
    [JsonPropertyName("wait_time")]
    public int? WaitTime { get; init; }

    /// <summary>The callback URL invoked when the batch finishes, when configured.</summary>
    [JsonPropertyName("callback_url")]
    public string? CallbackUrl { get; init; }

    /// <summary>The batch processing state (<c>processing</c>, <c>done</c>).</summary>
    [JsonPropertyName("state")]
    public string? State { get; init; }

    /// <summary>The per-browser screenshot results (populated by the detail endpoint).</summary>
    [JsonPropertyName("screenshots")]
    public IReadOnlyList<ScreenshotImage>? Screenshots { get; init; }
}
