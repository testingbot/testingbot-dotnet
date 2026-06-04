using System.Collections.Generic;
using TestingBot.Api.Http;

namespace TestingBot.Api;

/// <summary>Operations for TestingBot configuration data that does not require authentication.</summary>
public interface IConfigurationClient
{
    /// <summary>
    /// Returns the current list of public IPv4 addresses TestingBot test machines originate from,
    /// for firewall allow-listing (<c>GET /v1/configuration/ip-ranges</c>).
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>A flat list of IPv4 address strings.</returns>
    Task<IReadOnlyList<string>> GetIpRangesAsync(CancellationToken cancellationToken = default);
}

internal sealed class ConfigurationClient(TestingBotConnection connection) : ResourceClient(connection), IConfigurationClient
{
    public Task<IReadOnlyList<string>> GetIpRangesAsync(CancellationToken cancellationToken = default)
        => Connection.GetListAsync<string>("configuration/ip-ranges", query: null, cancellationToken);
}
