using System.Net.Http;

namespace TestingBot.Api.Http;

/// <summary>
/// Strongly typed <see cref="HttpRequestOptionsKey{T}"/> values the SDK attaches to individual
/// requests to influence the handler pipeline (per-request timeout, retry suppression).
/// </summary>
internal static class TestingBotRequestOptions
{
    /// <summary>Per-request timeout, applied per attempt by the retry handler.</summary>
    public static readonly HttpRequestOptionsKey<TimeSpan> Timeout = new("TestingBot.Timeout");

    /// <summary>When set to <see langword="true"/>, suppresses automatic retries (e.g. for streamed uploads).</summary>
    public static readonly HttpRequestOptionsKey<bool> DisableRetry = new("TestingBot.DisableRetry");
}
