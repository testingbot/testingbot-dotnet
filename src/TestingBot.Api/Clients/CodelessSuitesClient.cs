using System.Collections.Generic;
using System.Net.Http;
using TestingBot.Api.Http;
using TestingBot.Api.Models;
using TestingBot.Api.Serialization;

namespace TestingBot.Api;

/// <summary>Operations for Codeless suites via the <c>/v1/labsuites</c> endpoints.</summary>
public interface ICodelessSuitesClient
{
    /// <summary>Lists Codeless suites, newest first (<c>GET /v1/labsuites</c>).</summary>
    Task<TestingBotPage<CodelessSuite>> ListAsync(PageOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lazily iterates every Codeless suite across all pages.</summary>
    IAsyncEnumerable<CodelessSuite> ListAllAsync(int? pageSize = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a Codeless suite by id (<c>GET /v1/labsuites/:id</c>).</summary>
    Task<CodelessSuite> GetAsync(long suiteId, CancellationToken cancellationToken = default);

    /// <summary>Creates a Codeless suite (<c>POST /v1/labsuites</c>). Returns the new suite id.</summary>
    Task<long> CreateAsync(CodelessSuiteCreate request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a Codeless suite (<c>DELETE /v1/labsuites/:id</c>). Attached tests are preserved.</summary>
    Task<bool> DeleteAsync(long suiteId, CancellationToken cancellationToken = default);

    /// <summary>Runs every test in the suite (<c>POST /v1/labsuites/:id/trigger</c>).</summary>
    Task<TriggerResult> TriggerAsync(long suiteId, CancellationToken cancellationToken = default);

    /// <summary>Lists the Codeless tests in a suite (<c>GET /v1/labsuites/:id/tests</c>).</summary>
    Task<TestingBotPage<CodelessTest>> GetTestsAsync(long suiteId, PageOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Attaches existing Codeless tests to a suite (<c>POST /v1/labsuites/:id/tests</c>).</summary>
    Task<bool> AddTestsAsync(long suiteId, IReadOnlyList<long> testIds, CancellationToken cancellationToken = default);

    /// <summary>Detaches a Codeless test from a suite (<c>DELETE /v1/labsuites/:id/tests/:testId</c>).</summary>
    Task<bool> RemoveTestAsync(long suiteId, long testId, CancellationToken cancellationToken = default);

    /// <summary>Gets the browsers a suite runs on (<c>GET /v1/labsuites/:id/browsers</c>).</summary>
    Task<IReadOnlyList<Browser>> GetBrowsersAsync(long suiteId, CancellationToken cancellationToken = default);

    /// <summary>Replaces the browsers a suite runs on (<c>POST /v1/labsuites/:id/browsers</c>).</summary>
    Task<bool> SetBrowsersAsync(long suiteId, IReadOnlyList<int> browserIds, CancellationToken cancellationToken = default);
}

internal sealed class CodelessSuitesClient(TestingBotConnection connection) : ResourceClient(connection), ICodelessSuitesClient
{
    public Task<TestingBotPage<CodelessSuite>> ListAsync(PageOptions? options = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryString().Add("offset", options?.Offset).Add("count", options?.Count);
        return Connection.GetPageAsync<CodelessSuite>("labsuites", query, cancellationToken);
    }

    public IAsyncEnumerable<CodelessSuite> ListAllAsync(int? pageSize = null, CancellationToken cancellationToken = default)
        => PaginateAsync((offset, size, token) => ListAsync(new PageOptions { Offset = offset, Count = size }, token), pageSize, cancellationToken);

    public Task<CodelessSuite> GetAsync(long suiteId, CancellationToken cancellationToken = default)
        => Connection.GetAsync<CodelessSuite>($"labsuites/{suiteId}", query: null, cancellationToken);

    public async Task<long> CreateAsync(CodelessSuiteCreate request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var form = new FormContentBuilder()
            .Add("suite", "name", request.Name)
            .Add("suite", "cron", request.Cron)
            .Add("suite", "screenshot", request.Screenshot)
            .Add("suite", "video", request.Video)
            .Add("suite", "idletimeout", request.IdleTimeout)
            .Add("suite", "screenresolution", request.ScreenResolution);

        var element = await Connection.SendRawAsync(HttpMethod.Post, "labsuites", form.Build(), query: null, isUpload: false, cancellationToken).ConfigureAwait(false);
        return element.GetInt64OrDefault("suite_id");
    }

    public Task<bool> DeleteAsync(long suiteId, CancellationToken cancellationToken = default)
        => Connection.SendAckAsync(HttpMethod.Delete, $"labsuites/{suiteId}", content: null, query: null, cancellationToken);

    public async Task<TriggerResult> TriggerAsync(long suiteId, CancellationToken cancellationToken = default)
    {
        var element = await Connection.SendRawAsync(HttpMethod.Post, $"labsuites/{suiteId}/trigger", content: null, query: null, isUpload: false, cancellationToken).ConfigureAwait(false);
        return new TriggerResult { Success = element.GetBoolOrFalse("success"), JobId = element.GetInt64OrDefault("job_id") };
    }

    public Task<TestingBotPage<CodelessTest>> GetTestsAsync(long suiteId, PageOptions? options = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryString().Add("offset", options?.Offset).Add("count", options?.Count);
        return Connection.GetPageAsync<CodelessTest>($"labsuites/{suiteId}/tests", query, cancellationToken);
    }

    public Task<bool> AddTestsAsync(long suiteId, IReadOnlyList<long> testIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(testIds);
        var form = new FormContentBuilder().Add("test_ids", string.Join(',', testIds));
        return Connection.SendAckAsync(HttpMethod.Post, $"labsuites/{suiteId}/tests", form.Build(), query: null, cancellationToken);
    }

    public Task<bool> RemoveTestAsync(long suiteId, long testId, CancellationToken cancellationToken = default)
        => Connection.SendAckAsync(HttpMethod.Delete, $"labsuites/{suiteId}/tests/{testId}", content: null, query: null, cancellationToken);

    public Task<IReadOnlyList<Browser>> GetBrowsersAsync(long suiteId, CancellationToken cancellationToken = default)
        => Connection.GetListAsync<Browser>($"labsuites/{suiteId}/browsers", query: null, cancellationToken);

    public Task<bool> SetBrowsersAsync(long suiteId, IReadOnlyList<int> browserIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(browserIds);
        var form = new FormContentBuilder().Add("browser_ids", string.Join(',', browserIds));
        return Connection.SendAckAsync(HttpMethod.Post, $"labsuites/{suiteId}/browsers", form.Build(), query: null, cancellationToken);
    }
}
