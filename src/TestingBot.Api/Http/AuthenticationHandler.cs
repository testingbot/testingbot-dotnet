using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace TestingBot.Api.Http;

/// <summary>
/// Adds the HTTP Basic <c>Authorization</c> header (API key as username, secret as password)
/// to every outgoing request that does not already carry one.
/// </summary>
internal sealed class AuthenticationHandler : DelegatingHandler
{
    private readonly string _credentials;

    public AuthenticationHandler(string apiKey, string apiSecret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(apiKey);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiSecret);
        this._credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{apiKey}:{apiSecret}"));
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        request.Headers.Authorization ??= new AuthenticationHeaderValue("Basic", this._credentials);
        return base.SendAsync(request, cancellationToken);
    }
}
