using System.Collections.Generic;
using System.Net.Http;
using TestingBot.Api.Http;
using TestingBot.Api.Models;
using TestingBot.Api.Serialization;

namespace TestingBot.Api;

/// <summary>Operations for managing a TestingBot team and its sub-accounts.</summary>
public interface ITeamClient
{
    /// <summary>Gets allowed vs current concurrency for the team (<c>GET /v1/team-management</c>).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The concurrency snapshot.</returns>
    Task<TeamConcurrency> GetConcurrencyAsync(CancellationToken cancellationToken = default);

    /// <summary>Lists members of the team (<c>GET /v1/team-management/users</c>). Requires team admin.</summary>
    /// <param name="options">Optional pagination.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>A page of team members.</returns>
    Task<TestingBotPage<TeamMember>> ListUsersAsync(PageOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lazily iterates every team member across all pages.</summary>
    /// <param name="pageSize">Optional page size; pagination is handled automatically.</param>
    /// <param name="cancellationToken">A token to stop iteration.</param>
    /// <returns>An async sequence of team members.</returns>
    IAsyncEnumerable<TeamMember> ListUsersAllAsync(int? pageSize = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a team member by id (<c>GET /v1/team-management/users/:id</c>).</summary>
    /// <param name="userId">The numeric user id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The team member.</returns>
    Task<TeamMember> GetUserAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Gets a team member's API client key (<c>GET /v1/team-management/users/:id/client-key</c>). Requires team admin.</summary>
    /// <param name="userId">The numeric user id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The member's client key.</returns>
    Task<string?> GetUserClientKeyAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Rotates a team member's API credentials (<c>POST /v1/team-management/users/:id/reset-keys</c>).</summary>
    /// <param name="userId">The numeric user id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The reset result, including the new client key.</returns>
    Task<TeamCredentialReset> ResetUserKeysAsync(int userId, CancellationToken cancellationToken = default);

    /// <summary>Creates a team member (<c>POST /v1/team-management/users</c>). Requires a paid plan and team admin.</summary>
    /// <param name="request">The new member's attributes.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The created team member.</returns>
    Task<TeamMember> CreateUserAsync(TeamMemberCreate request, CancellationToken cancellationToken = default);

    /// <summary>Updates a team member (<c>PUT /v1/team-management/users/:id</c>).</summary>
    /// <param name="userId">The numeric user id.</param>
    /// <param name="request">The fields to change.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The updated team member.</returns>
    Task<TeamMember> UpdateUserAsync(int userId, TeamMemberUpdate request, CancellationToken cancellationToken = default);
}

internal sealed class TeamClient(TestingBotConnection connection) : ResourceClient(connection), ITeamClient
{
    public async Task<TeamConcurrency> GetConcurrencyAsync(CancellationToken cancellationToken = default)
    {
        var response = await Connection.GetAsync<TeamConcurrencyResponse>("team-management", query: null, cancellationToken).ConfigureAwait(false);
        return response.Concurrency ?? new TeamConcurrency();
    }

    public Task<TestingBotPage<TeamMember>> ListUsersAsync(PageOptions? options = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryString().Add("offset", options?.Offset).Add("count", options?.Count);
        return Connection.GetPageAsync<TeamMember>("team-management/users", query, cancellationToken);
    }

    public IAsyncEnumerable<TeamMember> ListUsersAllAsync(int? pageSize = null, CancellationToken cancellationToken = default)
        => PaginateAsync(
            (offset, size, token) => ListUsersAsync(new PageOptions { Offset = offset, Count = size }, token),
            pageSize,
            cancellationToken);

    public Task<TeamMember> GetUserAsync(int userId, CancellationToken cancellationToken = default)
        => Connection.GetAsync<TeamMember>($"team-management/users/{userId}", query: null, cancellationToken);

    public async Task<string?> GetUserClientKeyAsync(int userId, CancellationToken cancellationToken = default)
    {
        var element = await Connection.SendRawAsync(HttpMethod.Get, $"team-management/users/{userId}/client-key", content: null, query: null, isUpload: false, cancellationToken).ConfigureAwait(false);
        return element.GetStringOrNull("client_key");
    }

    public Task<TeamCredentialReset> ResetUserKeysAsync(int userId, CancellationToken cancellationToken = default)
        => Connection.SendForAsync<TeamCredentialReset>(HttpMethod.Post, $"team-management/users/{userId}/reset-keys", content: null, query: null, isUpload: false, cancellationToken);

    public Task<TeamMember> CreateUserAsync(TeamMemberCreate request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var form = new FormContentBuilder()
            .Add("email", request.Email)
            .Add("password", request.Password)
            .Add("first_name", request.FirstName)
            .Add("last_name", request.LastName)
            .Add("concurrency", request.Concurrency)
            .Add("concurrencyPhysical", request.ConcurrencyPhysical);
        return Connection.SendForAsync<TeamMember>(HttpMethod.Post, "team-management/users", form.Build(), query: null, isUpload: false, cancellationToken);
    }

    public Task<TeamMember> UpdateUserAsync(int userId, TeamMemberUpdate request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var form = new FormContentBuilder()
            .Add("first_name", request.FirstName)
            .Add("last_name", request.LastName)
            .Add("email", request.Email)
            .Add("password", request.Password)
            .Add("credits", request.Credits)
            .Add("device_credits", request.DeviceCredits)
            .Add("concurrency", request.Concurrency)
            .Add("concurrencyPhysical", request.ConcurrencyPhysical);
        return Connection.SendForAsync<TeamMember>(HttpMethod.Put, $"team-management/users/{userId}", form.Build(), query: null, isUpload: false, cancellationToken);
    }
}
