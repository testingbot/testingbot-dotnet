using System.Net.Http;
using TestingBot.Api.Http;

namespace TestingBot.Api;

public partial interface ITestsClient
{
    /// <summary>Updates a test's metadata (<c>PUT /v1/tests/:id</c>).</summary>
    /// <param name="idOrSessionId">The numeric test id or WebDriver session id.</param>
    /// <param name="update">The fields to change.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the update was applied.</returns>
    Task<bool> UpdateAsync(string idOrSessionId, TestUpdate update, CancellationToken cancellationToken = default);

    /// <summary>Stops a running test (<c>PUT /v1/tests/:id/stop</c>).</summary>
    /// <param name="idOrSessionId">The numeric test id or WebDriver session id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the test was stopped.</returns>
    Task<bool> StopAsync(string idOrSessionId, CancellationToken cancellationToken = default);

    /// <summary>Permanently deletes a test and its assets (<c>DELETE /v1/tests/:id</c>).</summary>
    /// <param name="idOrSessionId">The numeric test id or WebDriver session id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the test was deleted.</returns>
    Task<bool> DeleteAsync(string idOrSessionId, CancellationToken cancellationToken = default);

    /// <summary>Creates a manual test record outside the WebDriver flow (<c>POST /v1/tests</c>).</summary>
    /// <param name="test">The test attributes.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the record was created.</returns>
    Task<bool> CreateAsync(ManualTestCreate test, CancellationToken cancellationToken = default);
}

internal sealed partial class TestsClient
{
    public Task<bool> UpdateAsync(string idOrSessionId, TestUpdate update, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idOrSessionId);
        ArgumentNullException.ThrowIfNull(update);

        var form = new FormContentBuilder()
            .Add("test", "name", update.Name)
            .Add("test", "success", update.Success)
            .Add("test", "status_message", update.StatusMessage)
            .Add("test", "extra", update.Extra)
            .Add("test", "build", update.Build)
            .Add("test", "public", update.Public);

        if (update.Groups is { Count: > 0 } groups)
        {
            form.Add("groups", string.Join(',', groups));
        }

        return Connection.SendAckAsync(HttpMethod.Put, $"tests/{Uri.EscapeDataString(idOrSessionId)}", form.Build(), query: null, cancellationToken);
    }

    public Task<bool> StopAsync(string idOrSessionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idOrSessionId);
        return Connection.SendAckAsync(HttpMethod.Put, $"tests/{Uri.EscapeDataString(idOrSessionId)}/stop", content: null, query: null, cancellationToken);
    }

    public Task<bool> DeleteAsync(string idOrSessionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idOrSessionId);
        return Connection.SendAckAsync(HttpMethod.Delete, $"tests/{Uri.EscapeDataString(idOrSessionId)}", content: null, query: null, cancellationToken);
    }

    public Task<bool> CreateAsync(ManualTestCreate test, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(test);
        var form = new FormContentBuilder()
            .Add("test", "name", test.Name)
            .Add("test", "success", test.Success)
            .Add("test", "status_message", test.StatusMessage)
            .Add("test", "extra", test.Extra)
            .Add("test", "build", test.Build);

        return Connection.SendAckAsync(HttpMethod.Post, "tests", form.Build(), query: null, cancellationToken);
    }
}
