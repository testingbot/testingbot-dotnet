using RichardSzalay.MockHttp;
using TestingBot.Api.Http;

namespace TestingBot.Api.Tests.TestSupport;

internal static class TestConnectionFactory
{
    public const string BaseUrl = "https://api.testingbot.com/v1/";

    public static (TestingBotConnection Connection, MockHttpMessageHandler Mock) Create()
    {
        var mock = new MockHttpMessageHandler();
        var options = new TestingBotClientOptions { ApiKey = "k", ApiSecret = "s" };
        return (new TestingBotConnection(mock.ToHttpClient(), options), mock);
    }
}
