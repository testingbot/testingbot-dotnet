using System.Text.Json;
using TestingBot.Api.Serialization;

namespace TestingBot.Api.Tests.Serialization;

public class FlexibleBooleanConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new FlexibleBooleanConverter() } };

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("2", true)]
    [InlineData("\"true\"", true)]
    [InlineData("\"false\"", false)]
    [InlineData("\"1\"", true)]
    [InlineData("\"0\"", false)]
    [InlineData("\"yes\"", true)]
    [InlineData("\"\"", false)]
    public void Reads_truthy_and_falsy_representations(string json, bool expected)
        => JsonSerializer.Deserialize<bool>(json, Options).Should().Be(expected);

    [Fact]
    public void Writes_canonical_boolean()
        => JsonSerializer.Serialize(true, Options).Should().Be("true");

    [Fact]
    public void Null_maps_to_null_for_nullable_target()
        => JsonSerializer.Deserialize<bool?>("null", Options).Should().BeNull();

    [Fact]
    public void Invalid_string_throws()
    {
        var act = () => JsonSerializer.Deserialize<bool>("\"maybe\"", Options);
        act.Should().Throw<JsonException>();
    }
}

public class TolerantDateTimeOffsetConverterTests
{
    private static readonly JsonSerializerOptions Options = new() { Converters = { new TolerantDateTimeOffsetConverter() } };

    [Fact]
    public void Reads_iso8601_string()
    {
        var value = JsonSerializer.Deserialize<DateTimeOffset>("\"2020-01-02T03:04:05Z\"", Options);
        value.Should().Be(new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.Zero));
    }

    [Fact]
    public void Reads_unix_seconds()
    {
        var value = JsonSerializer.Deserialize<DateTimeOffset>("1577934245", Options);
        value.Should().Be(new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.Zero));
    }

    [Fact]
    public void Reads_unix_milliseconds_for_large_values()
    {
        var value = JsonSerializer.Deserialize<DateTimeOffset>("1577934245000", Options);
        value.Should().Be(new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.Zero));
    }

    [Fact]
    public void Null_maps_to_null_for_nullable_target()
        => JsonSerializer.Deserialize<DateTimeOffset?>("null", Options).Should().BeNull();

    [Fact]
    public void Writes_iso8601_utc()
    {
        var json = JsonSerializer.Serialize(new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.Zero), Options);
        json.Should().Be("\"2020-01-02T03:04:05Z\"");
    }
}

public class TestingBotJsonContextTests
{
    [Theory]
    [InlineData("{\"success\":true}", true)]
    [InlineData("{\"success\":1}", true)]
    [InlineData("{\"success\":\"true\"}", true)]
    [InlineData("{\"success\":0}", false)]
    [InlineData("{\"success\":false}", false)]
    public void Ack_payload_uses_flexible_boolean(string json, bool expected)
    {
        var ack = JsonSerializer.Deserialize(json, TestingBotJson.TypeInfo<AckPayload>());
        ack!.Success.Should().Be(expected);
    }
}
