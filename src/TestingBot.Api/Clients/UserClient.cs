using System.Net.Http;
using TestingBot.Api.Http;
using TestingBot.Api.Models;

namespace TestingBot.Api;

/// <summary>Operations for the authenticated TestingBot account.</summary>
public interface IUserClient
{
    /// <summary>Gets the authenticated account's profile (<c>GET /v1/user</c>).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The user.</returns>
    Task<User> GetAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the API key and secret for the authenticated account (<c>GET /v1/user/keys</c>).
    /// Treat the result as a credential — never log or embed it in client-side code.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The account's API key and secret.</returns>
    Task<UserKeys> GetKeysAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates the authenticated account's name (<c>PUT /v1/user</c>). Only the first and last
    /// name are mutable through this endpoint.
    /// </summary>
    /// <param name="update">The fields to change (at least one must be set).</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the update was applied.</returns>
    Task<bool> UpdateAsync(UserUpdate update, CancellationToken cancellationToken = default);
}

internal sealed class UserClient(TestingBotConnection connection) : ResourceClient(connection), IUserClient
{
    public Task<User> GetAsync(CancellationToken cancellationToken = default)
        => Connection.GetAsync<User>("user", query: null, cancellationToken);

    public Task<UserKeys> GetKeysAsync(CancellationToken cancellationToken = default)
        => Connection.GetAsync<UserKeys>("user/keys", query: null, cancellationToken);

    public Task<bool> UpdateAsync(UserUpdate update, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(update);
        var form = new FormContentBuilder()
            .Add("user", "first_name", update.FirstName)
            .Add("user", "last_name", update.LastName);
        return Connection.SendAckAsync(HttpMethod.Put, "user", form.Build(), query: null, cancellationToken);
    }
}
