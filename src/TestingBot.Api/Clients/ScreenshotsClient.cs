using System.Collections.Generic;
using System.Net.Http;
using TestingBot.Api.Http;
using TestingBot.Api.Models;

namespace TestingBot.Api;

/// <summary>Operations for cross-browser screenshots via the <c>/v1/screenshots</c> endpoints.</summary>
public interface IScreenshotsClient
{
    /// <summary>Lists screenshot batches, newest first (<c>GET /v1/screenshots</c>).</summary>
    Task<TestingBotPage<Screenshot>> ListAsync(PageOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lazily iterates every screenshot batch across all pages.</summary>
    IAsyncEnumerable<Screenshot> ListAllAsync(int? pageSize = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a screenshot batch's per-browser results (<c>GET /v1/screenshots/:id</c>).</summary>
    /// <param name="batchId">The numeric batch id.</param>
    /// <param name="excludeIds">Screenshot ids to exclude (useful for delta fetches).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The batch, including its per-browser results.</returns>
    Task<Screenshot> GetAsync(long batchId, IReadOnlyList<long>? excludeIds = null, CancellationToken cancellationToken = default);

    /// <summary>Queues a cross-browser screenshot of a URL (<c>POST /v1/screenshots</c>).</summary>
    /// <param name="request">The capture parameters.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The created batch; poll <see cref="GetAsync"/> for results.</returns>
    Task<Screenshot> CaptureAsync(ScreenshotRequest request, CancellationToken cancellationToken = default);
}

internal sealed class ScreenshotsClient(TestingBotConnection connection) : ResourceClient(connection), IScreenshotsClient
{
    public Task<TestingBotPage<Screenshot>> ListAsync(PageOptions? options = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryString().Add("offset", options?.Offset).Add("count", options?.Count);
        return Connection.GetPageAsync<Screenshot>("screenshots", query, cancellationToken);
    }

    public IAsyncEnumerable<Screenshot> ListAllAsync(int? pageSize = null, CancellationToken cancellationToken = default)
        => PaginateAsync((offset, size, token) => ListAsync(new PageOptions { Offset = offset, Count = size }, token), pageSize, cancellationToken);

    public Task<Screenshot> GetAsync(long batchId, IReadOnlyList<long>? excludeIds = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryString();
        if (excludeIds is { Count: > 0 })
        {
            query.Add("excludeIds", string.Join(',', excludeIds));
        }

        return Connection.GetAsync<Screenshot>($"screenshots/{batchId}", query, cancellationToken);
    }

    public Task<Screenshot> CaptureAsync(ScreenshotRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.BrowserIds.Count == 0)
        {
            throw new TestingBotValidationException("At least one browser id is required to capture a screenshot.");
        }

        var content = JsonContentFactory.Create(writer =>
        {
            writer.WriteStartObject();
            writer.WriteString("url", request.Url);
            writer.WriteString("resolution", request.Resolution);
            writer.WriteStartArray("browsers");
            foreach (var browserId in request.BrowserIds)
            {
                writer.WriteNumberValue(browserId);
            }

            writer.WriteEndArray();
            if (request.WaitTime.HasValue)
            {
                writer.WriteNumber("wait_time", request.WaitTime.Value);
            }

            if (request.FullPage.HasValue)
            {
                writer.WriteBoolean("fullpage", request.FullPage.Value);
            }

            if (request.CallbackUrl is not null)
            {
                writer.WriteString("callback_url", request.CallbackUrl);
            }

            writer.WriteEndObject();
        });

        return Connection.SendForAsync<Screenshot>(HttpMethod.Post, "screenshots", content, query: null, isUpload: false, cancellationToken);
    }
}
