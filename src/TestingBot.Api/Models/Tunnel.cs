using System.Text.Json;
using System.Text.Json.Serialization;

namespace TestingBot.Api.Models;

/// <summary>A TestingBot Tunnel, as returned by the <c>/v1/tunnel</c> endpoints.</summary>
public sealed record Tunnel
{
    /// <summary>The unique numeric tunnel id.</summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>The tunnel lifecycle state (e.g. <c>READY</c>, <c>STOPPED</c>).</summary>
    [JsonPropertyName("state")]
    public string? State { get; init; }

    /// <summary>When the tunnel was launched.</summary>
    [JsonPropertyName("launched")]
    public DateTimeOffset? Launched { get; init; }

    /// <summary>The public tunnel identifier surfaced in the dashboard.</summary>
    [JsonPropertyName("tunnel_id")]
    public string? TunnelId { get; init; }

    /// <summary>The custom name passed via <c>--tunnel-identifier</c> on the tunnel client.</summary>
    [JsonPropertyName("identifier")]
    public string? Identifier { get; init; }

    /// <summary>Free-form metadata reported by the tunnel binary (client version, OS, region).</summary>
    [JsonPropertyName("metadata")]
    public JsonElement? Metadata { get; init; }
}
