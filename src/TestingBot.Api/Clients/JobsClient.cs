using TestingBot.Api.Http;
using TestingBot.Api.Models;

namespace TestingBot.Api;

/// <summary>Operations for polling asynchronous jobs started by trigger endpoints.</summary>
public interface IJobsClient
{
    /// <summary>Gets a job's current status (<c>GET /v1/jobs/:id</c>).</summary>
    /// <param name="jobId">The numeric job id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The job.</returns>
    Task<Job> GetAsync(long jobId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Polls a job until it reaches a terminal state, an optional timeout elapses, or the
    /// <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="jobId">The numeric job id.</param>
    /// <param name="timeout">An optional overall timeout. When <see langword="null"/>, polls until cancelled.</param>
    /// <param name="pollInterval">The delay between polls. Defaults to 5 seconds.</param>
    /// <param name="progress">An optional receiver notified after each poll.</param>
    /// <param name="cancellationToken">A token to cancel the wait.</param>
    /// <returns>The completed job.</returns>
    Task<Job> WaitForCompletionAsync(
        long jobId,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null,
        IProgress<Job>? progress = null,
        CancellationToken cancellationToken = default);
}

internal sealed class JobsClient(TestingBotConnection connection) : ResourceClient(connection), IJobsClient
{
    private static readonly TimeSpan DefaultPollInterval = TimeSpan.FromSeconds(5);

    public Task<Job> GetAsync(long jobId, CancellationToken cancellationToken = default)
        => Connection.GetAsync<Job>($"jobs/{jobId}", query: null, cancellationToken);

    public async Task<Job> WaitForCompletionAsync(
        long jobId,
        TimeSpan? timeout = null,
        TimeSpan? pollInterval = null,
        IProgress<Job>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var interval = pollInterval ?? DefaultPollInterval;
        using var deadline = timeout.HasValue ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken) : null;
        deadline?.CancelAfter(timeout!.Value);
        var token = deadline?.Token ?? cancellationToken;

        try
        {
            while (true)
            {
                var job = await GetAsync(jobId, token).ConfigureAwait(false);
                progress?.Report(job);
                if (job.IsComplete)
                {
                    return job;
                }

                await Task.Delay(interval, token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (deadline is not null && deadline.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TestingBotApiException($"Job {jobId} did not reach a terminal state within {timeout!.Value}.");
        }
    }
}
