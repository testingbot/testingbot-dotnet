using System.Net;
using TestingBot.Api.Serialization;

namespace TestingBot.Api.Tests.Serialization;

public class TestingBotErrorParserTests
{
    private static readonly Uri RequestUri = new("https://api.testingbot.com/v1/user");

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, typeof(TestingBotAuthenticationException))]
    [InlineData(HttpStatusCode.PaymentRequired, typeof(TestingBotPaymentRequiredException))]
    [InlineData(HttpStatusCode.Forbidden, typeof(TestingBotForbiddenException))]
    [InlineData(HttpStatusCode.NotFound, typeof(TestingBotNotFoundException))]
    [InlineData(HttpStatusCode.BadRequest, typeof(TestingBotValidationException))]
    [InlineData(HttpStatusCode.TooManyRequests, typeof(TestingBotRateLimitException))]
    [InlineData(HttpStatusCode.InternalServerError, typeof(TestingBotApiException))]
    [InlineData(HttpStatusCode.BadGateway, typeof(TestingBotApiException))]
    public void Maps_status_codes_to_exception_types(HttpStatusCode statusCode, Type expectedType)
    {
        var ex = TestingBotErrorParser.CreateException(statusCode, "{\"error\":\"boom\"}", null, "GET", RequestUri);

        ex.Should().BeOfType(expectedType);
        ex.StatusCode.Should().Be(statusCode);
        ex.ApiMessage.Should().Be("boom");
        ex.RequestMethod.Should().Be("GET");
        ex.RequestUri.Should().Be(RequestUri);
        ex.Message.Should().Contain("boom");
    }

    [Fact]
    public void Reads_message_field_when_error_absent()
    {
        var ex = TestingBotErrorParser.CreateException(HttpStatusCode.InternalServerError, "{\"message\":\"server fell over\"}", null, "GET", RequestUri);
        ex.ApiMessage.Should().Be("server fell over");
    }

    [Fact]
    public void Parses_validation_errors_array()
    {
        var ex = TestingBotErrorParser.CreateException(
            HttpStatusCode.BadRequest,
            "{\"success\":false,\"errors\":[\"name is required\",\"url is invalid\"]}",
            null,
            "POST",
            RequestUri);

        ex.Should().BeOfType<TestingBotValidationException>();
        ex.ValidationErrors.Should().BeEquivalentTo("name is required", "url is invalid");
        ex.ApiMessage.Should().Contain("name is required");
    }

    [Fact]
    public void Parses_errors_encoded_as_json_string()
    {
        var ex = TestingBotErrorParser.CreateException(
            HttpStatusCode.BadRequest,
            "{\"success\":false,\"errors\":\"[\\\"boom\\\"]\"}",
            null,
            "POST",
            RequestUri);

        ex.ValidationErrors.Should().ContainSingle().Which.Should().Be("boom");
    }

    [Fact]
    public void Captures_retry_after_for_rate_limit()
    {
        var ex = (TestingBotRateLimitException)TestingBotErrorParser.CreateException(
            HttpStatusCode.TooManyRequests, "{\"error\":\"slow down\"}", TimeSpan.FromSeconds(42), "GET", RequestUri);

        ex.RetryAfter.Should().Be(TimeSpan.FromSeconds(42));
    }

    [Fact]
    public void Falls_back_to_raw_body_for_non_json()
    {
        var ex = TestingBotErrorParser.CreateException(HttpStatusCode.InternalServerError, "plain text error", null, "GET", RequestUri);
        ex.ApiMessage.Should().Be("plain text error");
        ex.RawBody.Should().Be("plain text error");
    }

    [Fact]
    public void Handles_empty_body()
    {
        var ex = TestingBotErrorParser.CreateException(HttpStatusCode.NotFound, null, null, "GET", RequestUri);
        ex.Should().BeOfType<TestingBotNotFoundException>();
        ex.Message.Should().Contain("404");
    }
}
