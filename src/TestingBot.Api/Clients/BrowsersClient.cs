using System.Collections.Generic;
using TestingBot.Api.Http;
using TestingBot.Api.Models;

namespace TestingBot.Api;

/// <summary>The automation protocol to list browsers for.</summary>
public enum BrowserType
{
    /// <summary>Modern WebDriver environments (the default).</summary>
    WebDriver,

    /// <summary>Legacy Selenium RC environments.</summary>
    Rc,
}

/// <summary>Operations for discovering the browsers available on the TestingBot grid.</summary>
public interface IBrowsersClient
{
    /// <summary>Lists supported browsers (<c>GET /v1/browsers</c>).</summary>
    /// <param name="type">Filter by automation protocol; defaults to WebDriver when omitted.</param>
    /// <param name="cancellationToken">A token to cancel the request.</param>
    /// <returns>The available browser environments.</returns>
    Task<IReadOnlyList<Browser>> ListAsync(BrowserType? type = null, CancellationToken cancellationToken = default);
}

internal sealed class BrowsersClient(TestingBotConnection connection) : ResourceClient(connection), IBrowsersClient
{
    public Task<IReadOnlyList<Browser>> ListAsync(BrowserType? type = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryString().Add("type", type switch
        {
            BrowserType.WebDriver => "webdriver",
            BrowserType.Rc => "rc",
            _ => null,
        });

        return Connection.GetListAsync<Browser>("browsers", query, cancellationToken);
    }
}
