using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;

namespace TestingBot.Api.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddTestingBot_registers_resolvable_client()
    {
        var services = new ServiceCollection();
        services.AddTestingBot(o =>
        {
            o.ApiKey = "k";
            o.ApiSecret = "s";
        });

        using var provider = services.BuildServiceProvider();
        var client = provider.GetService<ITestingBotClient>();

        client.Should().NotBeNull();
        client!.Tests.Should().NotBeNull();
    }

    [Fact]
    public void AddTestingBot_client_is_singleton()
    {
        var services = new ServiceCollection();
        services.AddTestingBot(o =>
        {
            o.ApiKey = "k";
            o.ApiSecret = "s";
        });

        using var provider = services.BuildServiceProvider();
        var a = provider.GetRequiredService<ITestingBotClient>();
        var b = provider.GetRequiredService<ITestingBotClient>();

        a.Should().BeSameAs(b);
    }

    [Fact]
    public void AddTestingBot_validates_missing_credentials_on_resolve()
    {
        var services = new ServiceCollection();
        services.AddTestingBot(_ => { });

        using var provider = services.BuildServiceProvider();
        var act = () => provider.GetRequiredService<ITestingBotClient>();

        act.Should().Throw<TestingBotConfigurationException>();
    }

    [Fact]
    public void AddTestingBot_binds_from_configuration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TestingBot:ApiKey"] = "ck",
                ["TestingBot:ApiSecret"] = "cs",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddTestingBot(configuration.GetSection("TestingBot"));

        using var provider = services.BuildServiceProvider();
        provider.GetService<ITestingBotClient>().Should().NotBeNull();
    }

    [Fact]
    public async Task AddTestingBot_pipeline_sends_authenticated_requests()
    {
        var mock = new MockHttpMessageHandler();
        mock.Expect(HttpMethod.Get, "https://api.testingbot.com/v1/user")
            .WithHeaders("Authorization", "Basic " + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("k:s")))
            .Respond("application/json", "{\"email\":\"a@b.c\"}");

        var services = new ServiceCollection();
        services.AddTestingBot(o =>
        {
            o.ApiKey = "k";
            o.ApiSecret = "s";
        });
        // Swap the primary handler for the mock while keeping the SDK's auth + retry handlers.
        services.AddHttpClient("TestingBot").ConfigurePrimaryHttpMessageHandler(() => mock);

        using var provider = services.BuildServiceProvider();
        var client = provider.GetRequiredService<ITestingBotClient>();

        var user = await client.User.GetAsync();

        user.Email.Should().Be("a@b.c");
        mock.VerifyNoOutstandingExpectation();
    }
}
