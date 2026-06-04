using System.Text.Json.Serialization;

namespace TestingBot.Api.Serialization;

/// <summary>
/// Loose projection of the various error envelopes the TestingBot API returns. The API is not
/// consistent: errors arrive as <c>{ "error": "..." }</c>, <c>{ "message": "..." }</c>, or
/// <c>{ "success": false, "errors": [...] }</c>. The <c>errors</c> field is parsed separately
/// (it may be an array or a JSON-encoded string) by <see cref="TestingBotErrorParser"/>.
/// </summary>
internal sealed record ApiErrorPayload
{
    [JsonPropertyName("error")]
    public string? Error { get; init; }

    [JsonPropertyName("message")]
    public string? Message { get; init; }

    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; init; }

    [JsonPropertyName("success")]
    public bool? Success { get; init; }
}

/// <summary>
/// Generic acknowledgement envelope (<c>{ "success": true }</c>) returned by many write endpoints.
/// </summary>
internal sealed record AckPayload
{
    [JsonPropertyName("success")]
    public bool Success { get; init; }
}
