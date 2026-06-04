using System.Text.Json.Serialization;

namespace TestingBot.Api.Models;

/// <summary>The authenticated account, as returned by <c>GET /v1/user</c>.</summary>
public sealed record User
{
    /// <summary>Given name on the account.</summary>
    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    /// <summary>Family name on the account.</summary>
    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }

    /// <summary>Account email address.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    /// <summary>Subscription plan identifier (e.g. <c>automated</c>, <c>automated pro</c>).</summary>
    [JsonPropertyName("plan")]
    public string? Plan { get; init; }

    /// <summary>Maximum number of parallel VM-based sessions allowed by the plan.</summary>
    [JsonPropertyName("max_concurrent")]
    public int? MaxConcurrent { get; init; }

    /// <summary>Maximum number of parallel physical-device sessions allowed by the plan.</summary>
    [JsonPropertyName("max_concurrent_mobile")]
    public int? MaxConcurrentMobile { get; init; }

    /// <summary>Remaining test seconds (credit balance) on the account.</summary>
    [JsonPropertyName("seconds")]
    public long? Seconds { get; init; }

    /// <summary>Timestamp of the most recent dashboard login.</summary>
    [JsonPropertyName("last_login")]
    public DateTimeOffset? LastLogin { get; init; }

    /// <summary>Optional company name for billing.</summary>
    [JsonPropertyName("company")]
    public string? Company { get; init; }

    /// <summary>Billing address line.</summary>
    [JsonPropertyName("street")]
    public string? Street { get; init; }

    /// <summary>Billing city.</summary>
    [JsonPropertyName("city")]
    public string? City { get; init; }

    /// <summary>Billing country.</summary>
    [JsonPropertyName("country")]
    public string? Country { get; init; }

    /// <summary>VAT number (EU only).</summary>
    [JsonPropertyName("vat")]
    public string? Vat { get; init; }
}

/// <summary>The API key and secret for the authenticated account, as returned by <c>GET /v1/user/keys</c>.</summary>
public sealed record UserKeys
{
    /// <summary>The API client key.</summary>
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    /// <summary>The API client secret. Treat as a credential — never log or persist it in client-side code.</summary>
    [JsonPropertyName("secret")]
    public string? Secret { get; init; }
}
