using System.Text.RegularExpressions;

namespace TestingBot.Api.Tests;

public class TestingBotClientTests
{
    [Fact]
    public void GetSharingAuthHash_matches_reference_md5()
    {
        using var client = new TestingBotClient("key", "secret");

        // Independently computed: md5("key:secret:abc").
        client.GetSharingAuthHash("abc").Should().Be("2d75a004d10fb154c3f71b873c7f608a");
    }

    [Fact]
    public void GetSharingAuthHash_is_lowercase_hex()
    {
        using var client = new TestingBotClient("k", "s");
        var hash = client.GetSharingAuthHash("12345");
        Regex.IsMatch(hash, "^[0-9a-f]{32}$").Should().BeTrue();
    }

    [Fact]
    public void Constructor_exposes_all_sub_clients()
    {
        using var client = new TestingBotClient("k", "s");

        client.Configuration.Should().NotBeNull();
        client.Browsers.Should().NotBeNull();
        client.Devices.Should().NotBeNull();
        client.User.Should().NotBeNull();
        client.Jobs.Should().NotBeNull();
        client.Tests.Should().NotBeNull();
        client.Builds.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_validates_credentials()
    {
        var act = () => new TestingBotClient(new TestingBotClientOptions());
        act.Should().Throw<TestingBotConfigurationException>();
    }

    [Fact]
    public void FromEnvironment_uses_resolved_credentials()
    {
        var original = (
            Environment.GetEnvironmentVariable("TESTINGBOT_KEY"),
            Environment.GetEnvironmentVariable("TESTINGBOT_SECRET"));
        try
        {
            Environment.SetEnvironmentVariable("TESTINGBOT_KEY", "envkey");
            Environment.SetEnvironmentVariable("TESTINGBOT_SECRET", "envsecret");

            using var client = TestingBotClient.FromEnvironment(o => o.BaseAddress = new Uri("https://example.test/v1/"));

            client.GetSharingAuthHash("x").Should().HaveLength(32);
        }
        finally
        {
            Environment.SetEnvironmentVariable("TESTINGBOT_KEY", original.Item1);
            Environment.SetEnvironmentVariable("TESTINGBOT_SECRET", original.Item2);
        }
    }
}
