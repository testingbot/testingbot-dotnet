using System.Collections.Generic;
using System.Net.Http;
using TestingBot.Api.Http;
using TestingBot.Api.Models;

namespace TestingBot.Api;

/// <summary>Operations for managing TestingBot Tunnels.</summary>
public interface ITunnelsClient
{
    /// <summary>Lists active tunnels for the account (<c>GET /v1/tunnel/list</c>).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The running tunnels.</returns>
    Task<IReadOnlyList<Tunnel>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets the account's currently active tunnel (<c>GET /v1/tunnel</c>).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The active tunnel.</returns>
    Task<Tunnel> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Gets a tunnel by id (<c>GET /v1/tunnel/:id</c>).</summary>
    /// <param name="tunnelId">The numeric tunnel id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The tunnel.</returns>
    Task<Tunnel> GetAsync(int tunnelId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Launches a new tunnel (<c>POST /v1/tunnel/create</c>). Most users start tunnels via the
    /// TestingBot Tunnel client; use this only if you manage tunnel VMs directly.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The created tunnel.</returns>
    Task<Tunnel> CreateAsync(CancellationToken cancellationToken = default);

    /// <summary>Stops a tunnel by id (<c>DELETE /v1/tunnel/:id</c>).</summary>
    /// <param name="tunnelId">The numeric tunnel id.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the tunnel was stopped.</returns>
    Task<bool> StopAsync(int tunnelId, CancellationToken cancellationToken = default);

    /// <summary>Stops the account's currently active tunnel (<c>DELETE /v1/tunnel</c>).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the tunnel was stopped.</returns>
    Task<bool> StopActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Checks tunnel-API reachability (<c>GET /v1/tunnel/isalive-check</c>).</summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns><see langword="true"/> when the API is reachable.</returns>
    Task<bool> IsAliveAsync(CancellationToken cancellationToken = default);
}

internal sealed class TunnelsClient(TestingBotConnection connection) : ResourceClient(connection), ITunnelsClient
{
    public Task<IReadOnlyList<Tunnel>> ListAsync(CancellationToken cancellationToken = default)
        => Connection.GetListAsync<Tunnel>("tunnel/list", query: null, cancellationToken);

    public Task<Tunnel> GetActiveAsync(CancellationToken cancellationToken = default)
        => Connection.GetAsync<Tunnel>("tunnel", query: null, cancellationToken);

    public Task<Tunnel> GetAsync(int tunnelId, CancellationToken cancellationToken = default)
        => Connection.GetAsync<Tunnel>($"tunnel/{tunnelId}", query: null, cancellationToken);

    public Task<Tunnel> CreateAsync(CancellationToken cancellationToken = default)
        => Connection.SendForAsync<Tunnel>(HttpMethod.Post, "tunnel/create", content: null, query: null, isUpload: false, cancellationToken);

    public async Task<bool> StopAsync(int tunnelId, CancellationToken cancellationToken = default)
    {
        await Connection.SendVoidAsync(HttpMethod.Delete, $"tunnel/{tunnelId}", content: null, query: null, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> StopActiveAsync(CancellationToken cancellationToken = default)
    {
        await Connection.SendVoidAsync(HttpMethod.Delete, "tunnel", content: null, query: null, cancellationToken).ConfigureAwait(false);
        return true;
    }

    public async Task<bool> IsAliveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await Connection.SendVoidAsync(HttpMethod.Get, "tunnel/isalive-check", content: null, query: null, cancellationToken).ConfigureAwait(false);
            return true;
        }
        catch (TestingBotException)
        {
            return false;
        }
    }
}
