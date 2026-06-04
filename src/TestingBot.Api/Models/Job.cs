using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TestingBot.Api.Models;

/// <summary>
/// The status of an asynchronous job (typically a Codeless test or suite run), as returned by
/// <c>GET /v1/jobs/:id</c>. Once <see cref="Status"/> is <c>FINISHED</c>, <see cref="Success"/>
/// and per-test results are populated.
/// </summary>
public sealed record Job
{
    /// <summary>The job state: <c>QUEUED</c>, <c>RUNNING</c>, <c>FINISHED</c>, or <c>FAILED</c>.</summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>When the job was created.</summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset? CreatedAt { get; init; }

    /// <summary>When the job was last updated.</summary>
    [JsonPropertyName("updated_at")]
    public DateTimeOffset? UpdatedAt { get; init; }

    /// <summary>Aggregate pass/fail across all triggered tests; <see langword="null"/> until finished.</summary>
    [JsonPropertyName("success")]
    public bool? Success { get; init; }

    /// <summary>The test ids spawned by this job.</summary>
    [JsonPropertyName("test_ids")]
    public IReadOnlyList<long>? TestIds { get; init; }

    /// <summary>Whether the job has reached a terminal state (<c>FINISHED</c> or <c>FAILED</c>).</summary>
    [JsonIgnore]
    public bool IsComplete =>
        string.Equals(Status, "FINISHED", StringComparison.OrdinalIgnoreCase)
        || string.Equals(Status, "FAILED", StringComparison.OrdinalIgnoreCase);
}
