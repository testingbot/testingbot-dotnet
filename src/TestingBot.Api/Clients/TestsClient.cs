using System.Collections.Generic;
using TestingBot.Api.Http;
using TestingBot.Api.Models;

namespace TestingBot.Api;

/// <summary>Read operations for test sessions. Write operations are added by the full client.</summary>
public partial interface ITestsClient
{
    /// <summary>Gets a single test by numeric id or WebDriver session id (<c>GET /v1/tests/:id</c>).</summary>
    /// <param name="idOrSessionId">The numeric test id or the WebDriver session id.</param>
    /// <param name="skipFields">Fields to omit from the response. The HTML steps field is always omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The test.</returns>
    Task<TestCase> GetAsync(string idOrSessionId, TestFieldSkip skipFields = TestFieldSkip.None, CancellationToken cancellationToken = default);

    /// <summary>Lists tests for the account, newest first (<c>GET /v1/tests</c>).</summary>
    /// <param name="options">Optional filters and pagination.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>A page of tests.</returns>
    Task<TestingBotPage<TestCase>> ListAsync(TestListOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lazily iterates every test across all pages.</summary>
    /// <param name="options">Optional filters; pagination is handled automatically.</param>
    /// <param name="cancellationToken">A token to stop iteration.</param>
    /// <returns>An async sequence of tests.</returns>
    IAsyncEnumerable<TestCase> ListAllAsync(TestListOptions? options = null, CancellationToken cancellationToken = default);
}

internal sealed partial class TestsClient(TestingBotConnection connection) : ResourceClient(connection), ITestsClient
{
    public Task<TestCase> GetAsync(string idOrSessionId, TestFieldSkip skipFields = TestFieldSkip.None, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idOrSessionId);

        // The SDK never surfaces the server-rendered HTML steps field, so always skip it.
        var query = new QueryString().Add("skip_fields", (skipFields | TestFieldSkip.Steps).ToQueryValue());
        return Connection.GetAsync<TestCase>($"tests/{Uri.EscapeDataString(idOrSessionId)}", query, cancellationToken);
    }

    public Task<TestingBotPage<TestCase>> ListAsync(TestListOptions? options = null, CancellationToken cancellationToken = default)
        => Connection.GetPageAsync<TestCase>("tests", BuildListQuery(options), cancellationToken);

    public IAsyncEnumerable<TestCase> ListAllAsync(TestListOptions? options = null, CancellationToken cancellationToken = default)
        => PaginateAsync(
            (offset, size, token) => ListAsync((options ?? new TestListOptions()) with { Offset = offset, Count = size }, token),
            options?.Count,
            cancellationToken);

    private static QueryString BuildListQuery(TestListOptions? options)
    {
        var query = new QueryString();
        if (options is null)
        {
            return query;
        }

        return query
            .Add("offset", options.Offset)
            .Add("count", options.Count)
            .Add("since", options.UpdatedSince?.ToUnixTimeSeconds())
            .Add("browser_id", options.BrowserId)
            .Add("group", options.Group)
            .Add("build", options.Build)
            .Add("skip_fields", options.SkipFields.ToQueryValue());
    }
}
