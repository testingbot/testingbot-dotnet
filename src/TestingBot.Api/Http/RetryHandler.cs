using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace TestingBot.Api.Http;

/// <summary>
/// Applies a per-attempt timeout and retries transient failures (HTTP 408/429/5xx and transport
/// errors) on idempotent requests, using exponential backoff with full jitter and honoring the
/// server's <c>Retry-After</c> header on rate-limit responses. Each attempt re-sends a buffered
/// clone of the original request so retries are safe; requests marked
/// <see cref="TestingBotRequestOptions.DisableRetry"/> (e.g. streamed uploads) are never retried.
/// </summary>
internal sealed class RetryHandler : DelegatingHandler
{
    private readonly int _maxRetries;
    private readonly TimeSpan _baseDelay;
    private readonly TimeSpan _defaultTimeout;
    private readonly bool _respectRetryAfter;

    public RetryHandler(TestingBotClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        this._maxRetries = options.MaxRetries;
        this._baseDelay = options.RetryBaseDelay;
        this._defaultTimeout = options.Timeout;
        this._respectRetryAfter = options.RespectRetryAfter;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var timeout = GetTimeout(request);
        var canRetry = this._maxRetries > 0 && !IsRetryDisabled(request) && IsIdempotent(request.Method);
        var maxAttempts = canRetry ? this._maxRetries + 1 : 1;

        for (var attempt = 0; ; attempt++)
        {
            var attemptRequest = canRetry ? await CloneAsync(request, cancellationToken).ConfigureAwait(false) : request;
            var isLastAttempt = attempt >= maxAttempts - 1;
            try
            {
                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(timeout);

                HttpResponseMessage response;
                try
                {
                    response = await base.SendAsync(attemptRequest, timeoutCts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // A timeout, not caller cancellation.
                    if (isLastAttempt)
                    {
                        throw new TestingBotApiException(
                            $"The request {request.Method} {request.RequestUri} timed out after {timeout.TotalSeconds:0.#}s.");
                    }

                    await DelayAsync(ComputeBackoff(attempt), cancellationToken).ConfigureAwait(false);
                    continue;
                }
                catch (HttpRequestException) when (!isLastAttempt)
                {
                    await DelayAsync(ComputeBackoff(attempt), cancellationToken).ConfigureAwait(false);
                    continue;
                }

                if (!isLastAttempt && ShouldRetryStatus(response.StatusCode))
                {
                    var delay = ComputeDelay(response, attempt);
                    response.Dispose();
                    await DelayAsync(delay, cancellationToken).ConfigureAwait(false);
                    continue;
                }

                return response;
            }
            finally
            {
                if (canRetry)
                {
                    attemptRequest.Dispose();
                }
            }
        }
    }

    private static bool IsIdempotent(HttpMethod method)
        => method == HttpMethod.Get
            || method == HttpMethod.Delete
            || method == HttpMethod.Put
            || method == HttpMethod.Head
            || method == HttpMethod.Options;

    private static bool IsRetryDisabled(HttpRequestMessage request)
        => request.Options.TryGetValue(TestingBotRequestOptions.DisableRetry, out var disabled) && disabled;

    private TimeSpan GetTimeout(HttpRequestMessage request)
        => request.Options.TryGetValue(TestingBotRequestOptions.Timeout, out var timeout) && timeout > TimeSpan.Zero
            ? timeout
            : this._defaultTimeout;

    private static bool ShouldRetryStatus(HttpStatusCode statusCode)
        => statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            or HttpStatusCode.InternalServerError
            or HttpStatusCode.BadGateway
            or HttpStatusCode.ServiceUnavailable
            or HttpStatusCode.GatewayTimeout;

    private TimeSpan ComputeDelay(HttpResponseMessage response, int attempt)
    {
        if (this._respectRetryAfter && response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            var retryAfter = response.Headers.RetryAfter;
            if (retryAfter?.Delta is { } delta && delta > TimeSpan.Zero)
            {
                return delta;
            }

            if (retryAfter?.Date is { } date)
            {
                var wait = date - DateTimeOffset.UtcNow;
                if (wait > TimeSpan.Zero)
                {
                    return wait;
                }
            }
        }

        return ComputeBackoff(attempt);
    }

    private TimeSpan ComputeBackoff(int attempt)
    {
        // Exponential backoff with full jitter: random delay in [0, base * 2^attempt].
        var exponential = this._baseDelay.TotalMilliseconds * Math.Pow(2, attempt);
        var capped = Math.Min(exponential, TimeSpan.FromSeconds(30).TotalMilliseconds);
        var jittered = Random.Shared.NextDouble() * capped;
        return TimeSpan.FromMilliseconds(jittered);
    }

    private static Task DelayAsync(TimeSpan delay, CancellationToken cancellationToken)
        => delay > TimeSpan.Zero ? Task.Delay(delay, cancellationToken) : Task.CompletedTask;

    private static async Task<HttpRequestMessage> CloneAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri) { Version = request.Version };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        foreach (var option in (IEnumerable<KeyValuePair<string, object?>>)request.Options)
        {
            ((IDictionary<string, object?>)clone.Options)[option.Key] = option.Value;
        }

        if (request.Content is not null)
        {
            var bytes = await request.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            var content = new ByteArrayContent(bytes);
            foreach (var header in request.Content.Headers)
            {
                content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            clone.Content = content;
        }

        return clone;
    }
}
