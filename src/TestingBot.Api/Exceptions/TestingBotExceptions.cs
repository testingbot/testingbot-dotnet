namespace TestingBot.Api;

/// <summary>
/// Raised when the SDK cannot resolve API credentials, or when client options are invalid.
/// This error is produced before any HTTP request is sent.
/// </summary>
public sealed class TestingBotConfigurationException : TestingBotException
{
    /// <summary>Initializes a new instance of the <see cref="TestingBotConfigurationException"/> class.</summary>
    /// <param name="message">A human-readable description of the configuration problem.</param>
    public TestingBotConfigurationException(string message)
        : base(message)
    {
    }
}

/// <summary>Raised when the API rejects the supplied credentials (HTTP 401).</summary>
public sealed class TestingBotAuthenticationException : TestingBotException
{
    /// <summary>Initializes a new instance of the <see cref="TestingBotAuthenticationException"/> class.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public TestingBotAuthenticationException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Raised when the account is not permitted to perform the operation (HTTP 403),
/// for example when the account is marked read-only or the caller is not a team admin.
/// </summary>
public sealed class TestingBotForbiddenException : TestingBotException
{
    /// <summary>Initializes a new instance of the <see cref="TestingBotForbiddenException"/> class.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public TestingBotForbiddenException(string message)
        : base(message)
    {
    }
}

/// <summary>Raised when the account has insufficient credits to perform the operation (HTTP 402).</summary>
public sealed class TestingBotPaymentRequiredException : TestingBotException
{
    /// <summary>Initializes a new instance of the <see cref="TestingBotPaymentRequiredException"/> class.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public TestingBotPaymentRequiredException(string message)
        : base(message)
    {
    }
}

/// <summary>Raised when the requested resource does not exist (HTTP 404).</summary>
public sealed class TestingBotNotFoundException : TestingBotException
{
    /// <summary>Initializes a new instance of the <see cref="TestingBotNotFoundException"/> class.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public TestingBotNotFoundException(string message)
        : base(message)
    {
    }
}

/// <summary>Raised when the API rejects the request as invalid (HTTP 400).</summary>
public sealed class TestingBotValidationException : TestingBotException
{
    /// <summary>Initializes a new instance of the <see cref="TestingBotValidationException"/> class.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public TestingBotValidationException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Raised when the account has exceeded its request rate limit (HTTP 429).
/// Inspect <see cref="RetryAfter"/> to learn how long to wait before retrying.
/// </summary>
public sealed class TestingBotRateLimitException : TestingBotException
{
    /// <summary>Initializes a new instance of the <see cref="TestingBotRateLimitException"/> class.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    public TestingBotRateLimitException(string message)
        : base(message)
    {
    }

    /// <summary>The delay advertised by the server's <c>Retry-After</c> header, when present.</summary>
    public TimeSpan? RetryAfter { get; init; }
}

/// <summary>
/// Raised for any other unsuccessful API response (typically 5xx, or an unmapped 4xx),
/// and for transport-level failures that survived the retry policy.
/// </summary>
public sealed class TestingBotApiException : TestingBotException
{
    /// <summary>Initializes a new instance of the <see cref="TestingBotApiException"/> class.</summary>
    /// <param name="message">A human-readable description of the error.</param>
    /// <param name="innerException">The underlying transport exception, if any.</param>
    public TestingBotApiException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
