using System.Net;
using System.Net.Http;
using RichardSzalay.MockHttp;
using TestingBot.Api.Http;
using TestingBot.Api.Serialization;

namespace TestingBot.Api.Tests.Http;

public class TestingBotConnectionTests
{
    private const string BaseUrl = "https://api.testingbot.com/v1/";

    private static (TestingBotConnection Connection, MockHttpMessageHandler Mock) Create()
    {
        var mock = new MockHttpMessageHandler();
        var options = new TestingBotClientOptions { ApiKey = "k", ApiSecret = "s" };
        var connection = new TestingBotConnection(mock.ToHttpClient(), options);
        return (connection, mock);
    }

    [Fact]
    public async Task GetListAsync_parses_bare_array()
    {
        var (connection, mock) = Create();
        mock.When(HttpMethod.Get, BaseUrl + "configuration/ip-ranges")
            .Respond("application/json", "[\"1.2.3.4\",\"5.6.7.8\"]");

        var list = await connection.GetListAsync<string>("configuration/ip-ranges");

        list.Should().BeEquivalentTo("1.2.3.4", "5.6.7.8");
    }

    [Fact]
    public async Task GetPageAsync_parses_data_and_meta_envelope()
    {
        var (connection, mock) = Create();
        mock.When(HttpMethod.Get, BaseUrl + "tests")
            .Respond("application/json", "{\"data\":[\"a\",\"b\"],\"meta\":{\"offset\":0,\"count\":2,\"total\":5}}");

        var page = await connection.GetPageAsync<string>("tests");

        page.Data.Should().BeEquivalentTo("a", "b");
        page.Meta.Total.Should().Be(5);
        page.Meta.HasMore.Should().BeTrue();
        page.Count.Should().Be(2);
    }

    [Fact]
    public async Task SendAckAsync_reads_success_flag()
    {
        var (connection, mock) = Create();
        mock.When(HttpMethod.Put, BaseUrl + "tests/1/stop")
            .Respond("application/json", "{\"success\":true}");

        var ok = await connection.SendAckAsync(HttpMethod.Put, "tests/1/stop");

        ok.Should().BeTrue();
    }

    [Fact]
    public async Task BuildUri_appends_query_string()
    {
        var (connection, mock) = Create();
        mock.Expect(HttpMethod.Get, BaseUrl + "tests")
            .WithExactQueryString("offset=10&count=5")
            .Respond("application/json", "{\"data\":[],\"meta\":{\"offset\":10,\"count\":0,\"total\":0}}");

        var query = new QueryString().Add("offset", 10).Add("count", 5);
        await connection.GetPageAsync<string>("tests", query);

        mock.VerifyNoOutstandingExpectation();
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, typeof(TestingBotAuthenticationException))]
    [InlineData(HttpStatusCode.PaymentRequired, typeof(TestingBotPaymentRequiredException))]
    [InlineData(HttpStatusCode.Forbidden, typeof(TestingBotForbiddenException))]
    [InlineData(HttpStatusCode.NotFound, typeof(TestingBotNotFoundException))]
    [InlineData(HttpStatusCode.BadRequest, typeof(TestingBotValidationException))]
    [InlineData(HttpStatusCode.InternalServerError, typeof(TestingBotApiException))]
    public async Task Maps_error_status_codes_to_typed_exceptions(HttpStatusCode statusCode, Type expectedType)
    {
        var (connection, mock) = Create();
        mock.When(HttpMethod.Get, BaseUrl + "user")
            .Respond(statusCode, "application/json", "{\"error\":\"nope\"}");

        var act = () => connection.GetAsync<AckPayload>("user");

        (await act.Should().ThrowAsync<TestingBotException>()).Which.Should().BeOfType(expectedType);
    }

    [Fact]
    public async Task Captures_retry_after_header_on_rate_limit()
    {
        var (connection, mock) = Create();
        mock.When(HttpMethod.Get, BaseUrl + "user").Respond(_ =>
        {
            var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
            {
                Content = new StringContent("{\"error\":\"slow down\"}"),
            };
            response.Headers.TryAddWithoutValidation("Retry-After", "30");
            return response;
        });

        var act = () => connection.GetAsync<AckPayload>("user");

        var assertion = await act.Should().ThrowAsync<TestingBotRateLimitException>();
        assertion.Which.RetryAfter.Should().Be(TimeSpan.FromSeconds(30));
    }
}
