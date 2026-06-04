using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using TestingBot.Api.Http;

namespace TestingBot.Api;

/// <summary>
/// The default <see cref="ITestingBotClient"/> implementation. Construct it directly with your API
/// key and secret, from a <see cref="TestingBotClientOptions"/>, or from the environment via
/// <see cref="FromEnvironment"/>. For dependency-injection scenarios, use the
/// <c>AddTestingBot</c> extension from the <c>TestingBot.Api.DependencyInjection</c> package instead.
/// </summary>
public sealed class TestingBotClient : ITestingBotClient, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly string _apiKey;
    private readonly string _apiSecret;

    /// <summary>Creates a client from an API key and secret, building a managed <see cref="HttpClient"/>.</summary>
    /// <param name="apiKey">The TestingBot API key.</param>
    /// <param name="apiSecret">The TestingBot API secret.</param>
    public TestingBotClient(string apiKey, string apiSecret)
        : this(new TestingBotClientOptions { ApiKey = apiKey, ApiSecret = apiSecret })
    {
    }

    /// <summary>Creates a client from options, building a managed <see cref="HttpClient"/>.</summary>
    /// <param name="options">The client options. Requires <see cref="TestingBotClientOptions.ApiKey"/> and <see cref="TestingBotClientOptions.ApiSecret"/>.</param>
    public TestingBotClient(TestingBotClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        this._apiKey = options.ApiKey!;
        this._apiSecret = options.ApiSecret!;
        this._httpClient = BuildHttpClient(options);
        this._ownsHttpClient = true;
        var connection = new TestingBotConnection(this._httpClient, options);
        InitializeClients(connection);
    }

    /// <summary>
    /// Creates a client over a caller-supplied <see cref="HttpClient"/> (used by dependency injection
    /// and tests). The handler pipeline — authentication, retries — is expected to be configured on the
    /// supplied client.
    /// </summary>
    internal TestingBotClient(HttpClient httpClient, TestingBotClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        this._apiKey = options.ApiKey!;
        this._apiSecret = options.ApiSecret!;
        this._httpClient = httpClient;
        this._ownsHttpClient = false;
        var connection = new TestingBotConnection(this._httpClient, options);
        InitializeClients(connection);
    }

    /// <summary>Creates a client using credentials resolved from the environment.</summary>
    /// <param name="configure">An optional callback to further configure options.</param>
    /// <returns>A configured client.</returns>
    /// <seealso cref="TestingBotCredentials"/>
    public static TestingBotClient FromEnvironment(Action<TestingBotClientOptions>? configure = null)
    {
        var (key, secret) = TestingBotCredentials.Resolve();
        var options = new TestingBotClientOptions { ApiKey = key, ApiSecret = secret };
        configure?.Invoke(options);
        return new TestingBotClient(options);
    }

    /// <inheritdoc />
    public IConfigurationClient Configuration { get; private set; } = null!;

    /// <inheritdoc />
    public IBrowsersClient Browsers { get; private set; } = null!;

    /// <inheritdoc />
    public IDevicesClient Devices { get; private set; } = null!;

    /// <inheritdoc />
    public IUserClient User { get; private set; } = null!;

    /// <inheritdoc />
    public IJobsClient Jobs { get; private set; } = null!;

    /// <inheritdoc />
    public ITestsClient Tests { get; private set; } = null!;

    /// <inheritdoc />
    public IBuildsClient Builds { get; private set; } = null!;

    /// <inheritdoc />
    public IStorageClient Storage { get; private set; } = null!;

    /// <inheritdoc />
    public IScreenshotsClient Screenshots { get; private set; } = null!;

    /// <inheritdoc />
    public ITunnelsClient Tunnels { get; private set; } = null!;

    /// <inheritdoc />
    public ICodelessTestsClient CodelessTests { get; private set; } = null!;

    /// <inheritdoc />
    public ICodelessSuitesClient CodelessSuites { get; private set; } = null!;

    /// <inheritdoc />
    public ITeamClient Team { get; private set; } = null!;

    /// <inheritdoc />
    public string GetSharingAuthHash(string identifier)
    {
        ArgumentNullException.ThrowIfNull(identifier);
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes($"{this._apiKey}:{this._apiSecret}:{identifier}"));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (this._ownsHttpClient)
        {
            this._httpClient.Dispose();
        }
    }

    private void InitializeClients(TestingBotConnection connection)
    {
        Configuration = new ConfigurationClient(connection);
        Browsers = new BrowsersClient(connection);
        Devices = new DevicesClient(connection);
        User = new UserClient(connection);
        Jobs = new JobsClient(connection);
        Tests = new TestsClient(connection);
        Builds = new BuildsClient(connection);
        Storage = new StorageClient(connection);
        Screenshots = new ScreenshotsClient(connection);
        Tunnels = new TunnelsClient(connection);
        CodelessTests = new CodelessTestsClient(connection);
        CodelessSuites = new CodelessSuitesClient(connection);
        Team = new TeamClient(connection);
    }

    private static HttpClient BuildHttpClient(TestingBotClientOptions options)
    {
        var primary = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            AutomaticDecompression = DecompressionMethods.All,
        };
        var retry = new RetryHandler(options) { InnerHandler = primary };
        var auth = new AuthenticationHandler(options.ApiKey!, options.ApiSecret!) { InnerHandler = retry };

        var httpClient = new HttpClient(auth, disposeHandler: true)
        {
            BaseAddress = options.BaseAddress,
            // Per-attempt timeouts are enforced by the retry handler; disable HttpClient's own timeout.
            Timeout = System.Threading.Timeout.InfiniteTimeSpan,
        };
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", TestingBotUserAgent.Resolve(options.UserAgent));
        httpClient.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        return httpClient;
    }
}
