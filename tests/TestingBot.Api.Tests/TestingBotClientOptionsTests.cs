namespace TestingBot.Api.Tests;

public class TestingBotClientOptionsTests
{
    [Fact]
    public void Base_address_gets_a_trailing_slash()
    {
        var options = new TestingBotClientOptions { BaseAddress = new Uri("https://example.test/v2") };
        options.BaseAddress.AbsoluteUri.Should().Be("https://example.test/v2/");
    }

    [Fact]
    public void Base_address_keeps_existing_trailing_slash()
    {
        var options = new TestingBotClientOptions { BaseAddress = new Uri("https://example.test/v2/") };
        options.BaseAddress.AbsoluteUri.Should().Be("https://example.test/v2/");
    }

    [Fact]
    public void Defaults_are_sensible()
    {
        var options = new TestingBotClientOptions();
        options.BaseAddress.Should().Be(TestingBotClientOptions.DefaultBaseAddress);
        options.MaxRetries.Should().Be(3);
        options.DefaultPageSize.Should().Be(50);
        options.Timeout.Should().Be(TimeSpan.FromSeconds(100));
        options.UploadTimeout.Should().Be(TimeSpan.FromMinutes(30));
    }

    [Fact]
    public void Validate_throws_when_credentials_missing()
    {
        var options = new TestingBotClientOptions();
        var act = () => options.Validate();
        act.Should().Throw<TestingBotConfigurationException>();
    }

    [Fact]
    public void Validate_passes_with_credentials()
    {
        var options = new TestingBotClientOptions { ApiKey = "k", ApiSecret = "s" };
        var act = () => options.Validate();
        act.Should().NotThrow();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(501)]
    public void Validate_rejects_out_of_range_page_size(int pageSize)
    {
        var options = new TestingBotClientOptions { ApiKey = "k", ApiSecret = "s", DefaultPageSize = pageSize };
        var act = () => options.Validate();
        act.Should().Throw<TestingBotConfigurationException>();
    }
}
