namespace TestingBot.Api;

/// <summary>
/// Configuration for a <c>TestingBotClient</c>. All properties have sensible defaults;
/// only <see cref="ApiKey"/> and <see cref="ApiSecret"/> are required (and may instead be
/// resolved from the environment — see <see cref="TestingBotCredentials"/>).
/// </summary>
public sealed class TestingBotClientOptions
{
    /// <summary>The default API base address, <c>https://api.testingbot.com/v1/</c>.</summary>
    public static readonly Uri DefaultBaseAddress = new("https://api.testingbot.com/v1/");

    private Uri _baseAddress = DefaultBaseAddress;

    /// <summary>The TestingBot API key (used as the HTTP Basic username).</summary>
    public string? ApiKey { get; set; }

    /// <summary>The TestingBot API secret (used as the HTTP Basic password).</summary>
    public string? ApiSecret { get; set; }

    /// <summary>
    /// The API base address. A trailing slash is enforced so that relative request paths
    /// combine correctly. Override this only to target a sandbox or private deployment.
    /// </summary>
    public Uri BaseAddress
    {
        get => this._baseAddress;
        set => this._baseAddress = EnsureTrailingSlash(value);
    }

    /// <summary>The per-request timeout for ordinary API calls. Defaults to 100 seconds.</summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(100);

    /// <summary>
    /// The timeout applied to storage uploads, which may transfer large binaries.
    /// Defaults to 30 minutes.
    /// </summary>
    public TimeSpan UploadTimeout { get; set; } = TimeSpan.FromMinutes(30);

    /// <summary>
    /// Maximum number of automatic retries for transient failures (HTTP 429 and 5xx,
    /// and transport errors) on idempotent requests. Defaults to 3. Set to 0 to disable retries.
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>The base delay for exponential backoff between retries. Defaults to 1 second.</summary>
    public TimeSpan RetryBaseDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// When <see langword="true"/> (the default), the retry policy honors the server's
    /// <c>Retry-After</c> header instead of its computed backoff delay.
    /// </summary>
    public bool RespectRetryAfter { get; set; } = true;

    /// <summary>
    /// An optional override for the <c>User-Agent</c> header. When unset, the SDK sends a
    /// descriptive default that includes the SDK and runtime versions.
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// The page size used by automatic pagination helpers when no explicit count is given.
    /// Defaults to 50. The API caps a single page at 500 items.
    /// </summary>
    public int DefaultPageSize { get; set; } = 50;

    /// <summary>Validates the options, throwing <see cref="TestingBotConfigurationException"/> on failure.</summary>
    internal void Validate()
    {
        if (string.IsNullOrWhiteSpace(ApiKey) || string.IsNullOrWhiteSpace(ApiSecret))
        {
            throw new TestingBotConfigurationException(
                "A TestingBot API key and secret are required. Pass them to the client, set them on " +
                "TestingBotClientOptions, or provide them via the TESTINGBOT_KEY/TESTINGBOT_SECRET " +
                "(or TB_KEY/TB_SECRET) environment variables or a ~/.testingbot file.");
        }

        if (DefaultPageSize is < 1 or > 500)
        {
            throw new TestingBotConfigurationException("DefaultPageSize must be between 1 and 500.");
        }

        if (MaxRetries < 0)
        {
            throw new TestingBotConfigurationException("MaxRetries cannot be negative.");
        }
    }

    private static Uri EnsureTrailingSlash(Uri uri)
    {
        ArgumentNullException.ThrowIfNull(uri);
        if (uri.AbsoluteUri.EndsWith('/'))
        {
            return uri;
        }

        return new Uri(uri.AbsoluteUri + "/");
    }
}
