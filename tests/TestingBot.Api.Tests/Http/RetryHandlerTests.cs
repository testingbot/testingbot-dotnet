using System.Net;
using System.Net.Http;
using TestingBot.Api.Http;
using TestingBot.Api.Tests.TestSupport;

namespace TestingBot.Api.Tests.Http;

public class RetryHandlerTests
{
    private static TestingBotClientOptions FastRetryOptions(int maxRetries = 3) => new()
    {
        ApiKey = "k",
        ApiSecret = "s",
        MaxRetries = maxRetries,
        RetryBaseDelay = TimeSpan.FromMilliseconds(1),
        Timeout = TimeSpan.FromSeconds(5),
    };

    private static HttpMessageInvoker InvokerFor(HttpMessageHandler inner, TestingBotClientOptions options)
        => new(new RetryHandler(options) { InnerHandler = inner });

    [Fact]
    public async Task Retries_transient_5xx_then_succeeds()
    {
        var stub = new StubHttpMessageHandler(
            StubHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable),
            StubHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable),
            StubHttpMessageHandler.Status(HttpStatusCode.OK));
        using var invoker = InvokerFor(stub, FastRetryOptions());

        using var response = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://x/"), CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stub.Calls.Should().Be(3);
    }

    [Fact]
    public async Task Stops_after_max_retries_and_returns_last_response()
    {
        var stub = new StubHttpMessageHandler(
            StubHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable),
            StubHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable),
            StubHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable),
            StubHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable));
        using var invoker = InvokerFor(stub, FastRetryOptions(maxRetries: 3));

        using var response = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://x/"), CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        stub.Calls.Should().Be(4); // 1 initial + 3 retries
    }

    [Fact]
    public async Task Does_not_retry_post_requests()
    {
        var stub = new StubHttpMessageHandler(StubHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable));
        using var invoker = InvokerFor(stub, FastRetryOptions());

        using var response = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Post, "https://x/"), CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        stub.Calls.Should().Be(1);
    }

    [Fact]
    public async Task Retries_rate_limit_responses()
    {
        var stub = new StubHttpMessageHandler(
            StubHttpMessageHandler.RateLimited(TimeSpan.Zero),
            StubHttpMessageHandler.Status(HttpStatusCode.OK));
        using var invoker = InvokerFor(stub, FastRetryOptions());

        using var response = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://x/"), CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stub.Calls.Should().Be(2);
    }

    [Fact]
    public async Task Honors_disable_retry_option()
    {
        var stub = new StubHttpMessageHandler(StubHttpMessageHandler.Status(HttpStatusCode.ServiceUnavailable));
        using var invoker = InvokerFor(stub, FastRetryOptions());
        var request = new HttpRequestMessage(HttpMethod.Get, "https://x/");
        request.Options.Set(TestingBotRequestOptions.DisableRetry, true);

        using var response = await invoker.SendAsync(request, CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        stub.Calls.Should().Be(1);
    }

    [Fact]
    public async Task Retries_on_timeout_then_succeeds()
    {
        var options = FastRetryOptions();
        options.Timeout = TimeSpan.FromMilliseconds(50);
        var stub = new StubHttpMessageHandler(
            StubHttpMessageHandler.TimesOut(),
            StubHttpMessageHandler.Status(HttpStatusCode.OK));
        using var invoker = InvokerFor(stub, options);

        using var response = await invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://x/"), CancellationToken.None);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stub.Calls.Should().Be(2);
    }

    [Fact]
    public async Task Throws_when_caller_cancels()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var stub = new StubHttpMessageHandler(StubHttpMessageHandler.Status(HttpStatusCode.OK));
        using var invoker = InvokerFor(stub, FastRetryOptions());

        var act = () => invoker.SendAsync(new HttpRequestMessage(HttpMethod.Get, "https://x/"), cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
