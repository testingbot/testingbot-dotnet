using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using TestingBot.Api;
using TestingBot.Api.Http;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency-injection registration for the TestingBot SDK. Registers <see cref="ITestingBotClient"/>
/// backed by <see cref="IHttpClientFactory"/> with the SDK's authentication and retry handlers.
/// </summary>
public static class TestingBotServiceCollectionExtensions
{
    private const string HttpClientName = "TestingBot";

    /// <summary>Registers the TestingBot client, configuring options inline.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">A callback to set the API key, secret, and any other options.</param>
    /// <returns>The service collection, for chaining.</returns>
    public static IServiceCollection AddTestingBot(this IServiceCollection services, Action<TestingBotClientOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configure);

        services.Configure(configure);
        return AddTestingBotCore(services);
    }

    /// <summary>Registers the TestingBot client, binding options from configuration.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">A configuration section containing <c>ApiKey</c>, <c>ApiSecret</c>, etc.</param>
    /// <returns>The service collection, for chaining.</returns>
    public static IServiceCollection AddTestingBot(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.Configure<TestingBotClientOptions>(configuration);
        return AddTestingBotCore(services);
    }

    private static IServiceCollection AddTestingBotCore(IServiceCollection services)
    {
        services.AddOptions<TestingBotClientOptions>().Validate(
            static options =>
            {
                options.Validate();
                return true;
            },
            "Invalid TestingBot client options.");

        services.TryAddTransient(static sp =>
        {
            var options = sp.GetRequiredService<IOptions<TestingBotClientOptions>>().Value;
            return new AuthenticationHandler(options.ApiKey!, options.ApiSecret!);
        });

        services.TryAddTransient(static sp =>
            new RetryHandler(sp.GetRequiredService<IOptions<TestingBotClientOptions>>().Value));

        services.AddHttpClient(HttpClientName, static (sp, http) =>
        {
            var options = sp.GetRequiredService<IOptions<TestingBotClientOptions>>().Value;
            options.Validate();
            http.BaseAddress = options.BaseAddress;
            // Per-attempt timeouts are enforced by the retry handler.
            http.Timeout = System.Threading.Timeout.InfiniteTimeSpan;
            http.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", TestingBotUserAgent.Resolve(options.UserAgent));
            http.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "application/json");
        })
        .ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(2),
            AutomaticDecompression = DecompressionMethods.All,
        })
        .AddHttpMessageHandler<AuthenticationHandler>()
        .AddHttpMessageHandler<RetryHandler>();

        services.TryAddSingleton<ITestingBotClient>(static sp =>
        {
            var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName);
            var options = sp.GetRequiredService<IOptions<TestingBotClientOptions>>().Value;
            return new TestingBotClient(httpClient, options);
        });

        return services;
    }
}
