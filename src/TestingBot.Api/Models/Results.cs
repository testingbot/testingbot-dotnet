namespace TestingBot.Api.Models;

/// <summary>The result of triggering a Codeless test or suite run.</summary>
public sealed record TriggerResult
{
    /// <summary>Whether the run was queued successfully.</summary>
    public bool Success { get; init; }

    /// <summary>The job id to poll via the jobs client for progress and results.</summary>
    public long JobId { get; init; }
}
