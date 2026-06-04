using System.Collections.Generic;
using System.Net;

namespace TestingBot.Api;

/// <summary>
/// Base type for every error raised by the TestingBot SDK. Catch this to handle all
/// SDK failures uniformly, or catch one of the derived types to react to a specific
/// failure mode (authentication, rate limiting, validation, and so on).
/// </summary>
public class TestingBotException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="TestingBotException"/> class.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="innerException">The underlying exception, if any.</param>
    public TestingBotException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    /// <summary>The HTTP status code returned by the API, when the error originated from an HTTP response.</summary>
    public HttpStatusCode? StatusCode { get; init; }

    /// <summary>The message extracted from the API error body, when available.</summary>
    public string? ApiMessage { get; init; }

    /// <summary>An API-specific error code, when the response provided one.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>The raw, unparsed response body, useful for diagnostics.</summary>
    public string? RawBody { get; init; }

    /// <summary>The HTTP method of the failing request (e.g. <c>GET</c>).</summary>
    public string? RequestMethod { get; init; }

    /// <summary>The absolute URI of the failing request.</summary>
    public Uri? RequestUri { get; init; }

    /// <summary>Field-level validation messages parsed from the API error body, when present.</summary>
    public IReadOnlyList<string> ValidationErrors { get; init; } = [];
}
