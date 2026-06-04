using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using TestingBot.Api.Serialization;

namespace TestingBot.Api.Http;

/// <summary>
/// Internal HTTP plumbing shared by every resource client: it builds requests against the
/// configured base address, sends them through the <see cref="HttpClient"/> pipeline, maps
/// unsuccessful responses to typed exceptions, and deserializes successful responses.
/// </summary>
internal sealed class TestingBotConnection
{
    private readonly HttpClient _httpClient;

    public TestingBotConnection(HttpClient httpClient, TestingBotClientOptions options)
    {
        this._httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public TestingBotClientOptions Options { get; }

    public Task<T> GetAsync<T>(string path, QueryString? query = null, CancellationToken cancellationToken = default)
        => SendForAsync<T>(HttpMethod.Get, path, content: null, query, isUpload: false, cancellationToken);

    public async Task<IReadOnlyList<T>> GetListAsync<T>(string path, QueryString? query = null, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, path, content: null, query, isUpload: false, cancellationToken).ConfigureAwait(false);
        return await DeserializeAsync<List<T>>(response, cancellationToken).ConfigureAwait(false);
    }

    public async Task<TestingBotPage<T>> GetPageAsync<T>(string path, QueryString? query = null, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Get, path, content: null, query, isUpload: false, cancellationToken).ConfigureAwait(false);
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            using var document = await JsonDocument.ParseAsync(stream, default, cancellationToken).ConfigureAwait(false);
            var root = document.RootElement;

            List<T> data = root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("data", out var dataElement)
                && dataElement.ValueKind == JsonValueKind.Array
                    ? dataElement.Deserialize(TestingBotJson.TypeInfo<List<T>>()) ?? []
                    : [];

            PageMeta meta = root.ValueKind == JsonValueKind.Object
                && root.TryGetProperty("meta", out var metaElement)
                && metaElement.ValueKind == JsonValueKind.Object
                    ? metaElement.Deserialize(TestingBotJson.TypeInfo<PageMeta>()) ?? new PageMeta()
                    : new PageMeta { Count = data.Count, Total = data.Count };

            return new TestingBotPage<T>(data, meta);
        }
    }

    public async Task<bool> SendAckAsync(
        HttpMethod method,
        string path,
        HttpContent? content = null,
        QueryString? query = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(method, path, content, query, isUpload: false, cancellationToken).ConfigureAwait(false);
        var ack = await DeserializeAsync<AckPayload>(response, cancellationToken).ConfigureAwait(false);
        return ack.Success;
    }

    public async Task<T> SendForAsync<T>(
        HttpMethod method,
        string path,
        HttpContent? content = null,
        QueryString? query = null,
        bool isUpload = false,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(method, path, content, query, isUpload, cancellationToken).ConfigureAwait(false);
        return await DeserializeAsync<T>(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Sends a request and succeeds on any 2xx response (the body is ignored).</summary>
    public async Task SendVoidAsync(
        HttpMethod method,
        string path,
        HttpContent? content = null,
        QueryString? query = null,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(method, path, content, query, isUpload: false, cancellationToken).ConfigureAwait(false);
    }

    public async Task<JsonElement> SendRawAsync(
        HttpMethod method,
        string path,
        HttpContent? content = null,
        QueryString? query = null,
        bool isUpload = false,
        CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(method, path, content, query, isUpload, cancellationToken).ConfigureAwait(false);
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            using var document = await JsonDocument.ParseAsync(stream, default, cancellationToken).ConfigureAwait(false);
            return document.RootElement.Clone();
        }
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string path,
        HttpContent? content,
        QueryString? query,
        bool isUpload,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(method, BuildUri(path, query)) { Content = content };
        request.Options.Set(TestingBotRequestOptions.Timeout, isUpload ? Options.UploadTimeout : Options.Timeout);
        if (isUpload)
        {
            request.Options.Set(TestingBotRequestOptions.DisableRetry, true);
        }

        HttpResponseMessage response;
        try
        {
            response = await this._httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (TestingBotException)
        {
            throw;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (HttpRequestException ex)
        {
            throw new TestingBotApiException($"The request {method} {request.RequestUri} failed: {ex.Message}", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            try
            {
                await ThrowForErrorAsync(method, request.RequestUri, response, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                response.Dispose();
            }
        }

        return response;
    }

    private Uri BuildUri(string path, QueryString? query)
    {
        var relative = path.TrimStart('/');
        var combined = new Uri(Options.BaseAddress, relative);
        if (query is null || query.IsEmpty)
        {
            return combined;
        }

        var builder = new UriBuilder(combined) { Query = query.ToString() };
        return builder.Uri;
    }

    private static async Task ThrowForErrorAsync(HttpMethod method, Uri? uri, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        string? body = null;
        try
        {
            body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception)
        {
            // Diagnostics only; fall through with a null body.
        }

        TimeSpan? retryAfter = null;
        var retryAfterHeader = response.Headers.RetryAfter;
        if (retryAfterHeader?.Delta is { } delta)
        {
            retryAfter = delta;
        }
        else if (retryAfterHeader?.Date is { } date)
        {
            var wait = date - DateTimeOffset.UtcNow;
            if (wait > TimeSpan.Zero)
            {
                retryAfter = wait;
            }
        }

        throw TestingBotErrorParser.CreateException(response.StatusCode, body, retryAfter, method.Method, uri);
    }

    private static async Task<T> DeserializeAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        {
            T? result;
            try
            {
                result = await JsonSerializer
                    .DeserializeAsync(stream, TestingBotJson.TypeInfo<T>(), cancellationToken)
                    .ConfigureAwait(false);
            }
            catch (JsonException ex)
            {
                throw new TestingBotApiException($"Failed to deserialize the API response: {ex.Message}", ex);
            }

            if (result is null)
            {
                throw new TestingBotApiException("The API returned an empty or null response where a value was expected.");
            }

            return result;
        }
    }
}
