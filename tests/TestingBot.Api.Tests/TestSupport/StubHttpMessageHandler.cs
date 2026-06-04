using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace TestingBot.Api.Tests.TestSupport;

/// <summary>
/// A scripted <see cref="HttpMessageHandler"/> that returns a queued sequence of behaviors,
/// recording every call. Used to drive handler-level tests (e.g. retry behavior).
/// </summary>
internal sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>> _behaviors;

    public StubHttpMessageHandler(params Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>[] behaviors)
    {
        this._behaviors = new Queue<Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>>>(behaviors);
    }

    public int Calls { get; private set; }

    public List<HttpMethod> Methods { get; } = [];

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Calls++;
        Methods.Add(request.Method);
        var behavior = this._behaviors.Count > 1 ? this._behaviors.Dequeue() : this._behaviors.Peek();
        return await behavior(request, cancellationToken);
    }

    public static Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> Status(HttpStatusCode statusCode)
        => (request, _) => Task.FromResult(new HttpResponseMessage(statusCode) { RequestMessage = request });

    public static Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> RateLimited(TimeSpan retryAfter)
        => (request, _) =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests) { RequestMessage = request };
            response.Headers.TryAddWithoutValidation("Retry-After", ((int)retryAfter.TotalSeconds).ToString(System.Globalization.CultureInfo.InvariantCulture));
            return Task.FromResult(response);
        };

    public static Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> TimesOut()
        => async (_, cancellationToken) =>
        {
            await Task.Delay(System.Threading.Timeout.Infinite, cancellationToken);
            return new HttpResponseMessage(HttpStatusCode.OK);
        };
}
