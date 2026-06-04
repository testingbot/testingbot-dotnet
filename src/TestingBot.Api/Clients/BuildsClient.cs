using System.Collections.Generic;
using System.Net.Http;
using TestingBot.Api.Http;
using TestingBot.Api.Models;

namespace TestingBot.Api;

/// <summary>Operations for builds (aggregations of tests sharing a <c>capabilities.build</c> identifier).</summary>
public interface IBuildsClient
{
    /// <summary>Lists builds for the account, newest first (<c>GET /v1/builds</c>).</summary>
    /// <param name="options">Optional pagination.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>A page of builds.</returns>
    Task<TestingBotPage<Build>> ListAsync(PageOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lazily iterates every build across all pages.</summary>
    /// <param name="pageSize">Optional page size; pagination is handled automatically.</param>
    /// <param name="cancellationToken">A token to stop iteration.</param>
    /// <returns>An async sequence of builds.</returns>
    IAsyncEnumerable<Build> ListAllAsync(int? pageSize = null, CancellationToken cancellationToken = default);

    /// <summary>Gets the tests belonging to a build (<c>GET /v1/builds/:id</c>).</summary>
    /// <param name="buildIdentifier">The numeric build id or the string build identifier.</param>
    /// <param name="options">Optional filters and pagination.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>A page of tests in the build.</returns>
    Task<TestingBotPage<TestCase>> GetTestsAsync(string buildIdentifier, BuildTestsOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lazily iterates every test in a build across all pages.</summary>
    /// <param name="buildIdentifier">The numeric build id or the string build identifier.</param>
    /// <param name="skipFields">Fields to omit from each test.</param>
    /// <param name="pageSize">Optional page size; pagination is handled automatically.</param>
    /// <param name="cancellationToken">A token to stop iteration.</param>
    /// <returns>An async sequence of tests in the build.</returns>
    IAsyncEnumerable<TestCase> GetTestsAllAsync(string buildIdentifier, TestFieldSkip skipFields = TestFieldSkip.None, int? pageSize = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes a build and all of its tests and assets (<c>DELETE /v1/builds/:id</c>).</summary>
    /// <param name="buildIdentifier">The numeric build id or the string build identifier.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the build was deleted.</returns>
    Task<bool> DeleteAsync(string buildIdentifier, CancellationToken cancellationToken = default);
}

internal sealed class BuildsClient(TestingBotConnection connection) : ResourceClient(connection), IBuildsClient
{
    public Task<TestingBotPage<Build>> ListAsync(PageOptions? options = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryString().Add("offset", options?.Offset).Add("count", options?.Count);
        return Connection.GetPageAsync<Build>("builds", query, cancellationToken);
    }

    public IAsyncEnumerable<Build> ListAllAsync(int? pageSize = null, CancellationToken cancellationToken = default)
        => PaginateAsync(
            (offset, size, token) => ListAsync(new PageOptions { Offset = offset, Count = size }, token),
            pageSize,
            cancellationToken);

    public Task<TestingBotPage<TestCase>> GetTestsAsync(string buildIdentifier, BuildTestsOptions? options = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildIdentifier);
        var query = new QueryString()
            .Add("offset", options?.Offset)
            .Add("count", options?.Count)
            .Add("skip_fields", options?.SkipFields.ToQueryValue());
        return Connection.GetPageAsync<TestCase>($"builds/{EscapePathPreservingSlash(buildIdentifier)}", query, cancellationToken);
    }

    public IAsyncEnumerable<TestCase> GetTestsAllAsync(string buildIdentifier, TestFieldSkip skipFields = TestFieldSkip.None, int? pageSize = null, CancellationToken cancellationToken = default)
        => PaginateAsync(
            (offset, size, token) => GetTestsAsync(buildIdentifier, new BuildTestsOptions { Offset = offset, Count = size, SkipFields = skipFields }, token),
            pageSize,
            cancellationToken);

    public Task<bool> DeleteAsync(string buildIdentifier, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildIdentifier);
        return Connection.SendAckAsync(HttpMethod.Delete, $"builds/{EscapePathPreservingSlash(buildIdentifier)}", content: null, query: null, cancellationToken);
    }
}
