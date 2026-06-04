using TestingBot.Api;

namespace TestingBot.Api.IntegrationTests;

/// <summary>
/// Read-only smoke tests against the live TestingBot API. Gated by <see cref="IntegrationFactAttribute"/>;
/// these never run unless integration testing is explicitly enabled.
/// </summary>
public class SmokeTests
{
    private static TestingBotClient CreateClient() => TestingBotClient.FromEnvironment();

    [IntegrationFact]
    public async Task GetUser_returns_account()
    {
        using var client = CreateClient();
        var user = await client.User.GetAsync();
        user.Should().NotBeNull();
        user.Email.Should().NotBeNullOrWhiteSpace();
    }

    [IntegrationFact]
    public async Task ListBrowsers_returns_environments()
    {
        using var client = CreateClient();
        var browsers = await client.Browsers.ListAsync();
        browsers.Should().NotBeEmpty();
    }

    [IntegrationFact]
    public async Task GetIpRanges_returns_addresses()
    {
        using var client = CreateClient();
        var ranges = await client.Configuration.GetIpRangesAsync();
        ranges.Should().NotBeEmpty();
    }

    [IntegrationFact]
    public async Task ListTests_paginates()
    {
        using var client = CreateClient();
        var page = await client.Tests.ListAsync(new TestListOptions { Count = 5 });
        page.Should().NotBeNull();
        page.Meta.Should().NotBeNull();
    }
}
