using System.Text.Json.Serialization;

namespace TestingBot.Api.Models;

/// <summary>A pair of concurrency counts split across VMs and physical devices.</summary>
public sealed record ConcurrencySlots
{
    /// <summary>VM-based concurrent sessions.</summary>
    [JsonPropertyName("vms")]
    public int Vms { get; init; }

    /// <summary>Physical-device concurrent sessions.</summary>
    [JsonPropertyName("physical")]
    public int Physical { get; init; }
}

/// <summary>The team's concurrency snapshot: allowed versus currently used.</summary>
public sealed record TeamConcurrency
{
    /// <summary>The maximum simultaneous sessions the plan allows.</summary>
    [JsonPropertyName("allowed")]
    public ConcurrencySlots? Allowed { get; init; }

    /// <summary>The sessions in use right now.</summary>
    [JsonPropertyName("current")]
    public ConcurrencySlots? Current { get; init; }
}

/// <summary>Envelope returned by <c>GET /v1/team-management</c>.</summary>
public sealed record TeamConcurrencyResponse
{
    /// <summary>The concurrency snapshot.</summary>
    [JsonPropertyName("concurrency")]
    public TeamConcurrency? Concurrency { get; init; }
}

/// <summary>A member (sub-account) of a TestingBot team.</summary>
public sealed record TeamMember
{
    /// <summary>The unique numeric user id.</summary>
    [JsonPropertyName("id")]
    public int Id { get; init; }

    /// <summary>Given name.</summary>
    [JsonPropertyName("first_name")]
    public string? FirstName { get; init; }

    /// <summary>Family name.</summary>
    [JsonPropertyName("last_name")]
    public string? LastName { get; init; }

    /// <summary>Account email.</summary>
    [JsonPropertyName("email")]
    public string? Email { get; init; }

    /// <summary>Remaining VM credit seconds.</summary>
    [JsonPropertyName("credits")]
    public long? Credits { get; init; }

    /// <summary>Remaining physical-device credit seconds.</summary>
    [JsonPropertyName("device_credits")]
    public long? DeviceCredits { get; init; }

    /// <summary>Whether the account has an active paid plan.</summary>
    [JsonPropertyName("isPaid")]
    public bool? IsPaid { get; init; }

    /// <summary>Whether the account email has been verified.</summary>
    [JsonPropertyName("verified")]
    public bool? Verified { get; init; }

    /// <summary>The parent (team owner) user id; <c>0</c> means this user is the team owner.</summary>
    [JsonPropertyName("parent_id")]
    public int? ParentId { get; init; }
}

/// <summary>The result of rotating a team member's API credentials.</summary>
public sealed record TeamCredentialReset
{
    /// <summary>Whether the credentials were rotated.</summary>
    [JsonPropertyName("success")]
    public bool Success { get; init; }

    /// <summary>The new API client key. The previous credentials stop working immediately.</summary>
    [JsonPropertyName("client_key")]
    public string? ClientKey { get; init; }
}
