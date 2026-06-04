using System.Text.Json;
using TestingBot.Api.Models;
using TestingBot.Api.Serialization;

namespace TestingBot.Api.Tests.Serialization;

public class ModelDeserializationTests
{
    private static T Deserialize<T>(string json) => JsonSerializer.Deserialize(json, TestingBotJson.TypeInfo<T>())!;

    [Fact]
    public void Browser_maps_camel_and_snake_case_keys()
    {
        const string json = """
        {"selenium_name":"chrome","name":"chrome","platform":"WIN10","browser_id":42,
         "version":"121","long_version":"121.0.1","deviceName":"iPhone 15","platformName":"iOS"}
        """;

        var browser = Deserialize<Browser>(json);

        browser.SeleniumName.Should().Be("chrome");
        browser.BrowserId.Should().Be(42);
        browser.Version.Should().Be("121");
        browser.DeviceName.Should().Be("iPhone 15");
        browser.PlatformName.Should().Be("iOS");
    }

    [Fact]
    public void Device_maps_fields_and_availability()
    {
        const string json = """
        {"id":7,"name":"iPhone 15 Pro","manufacturer":"Apple","platform_name":"iOS",
         "platform_version":"17.4","screen_resolution":"1170x2532","available":true,"free_trial":false}
        """;

        var device = Deserialize<Device>(json);

        device.Id.Should().Be(7);
        device.Name.Should().Be("iPhone 15 Pro");
        device.PlatformVersion.Should().Be("17.4");
        device.Available.Should().BeTrue();
        device.FreeTrial.Should().BeFalse();
    }

    [Fact]
    public void Build_maps_inline_shape()
    {
        const string json = """{"id":99,"build_identifier":"ci-1421","created_at":"2024-01-02T03:04:05Z","updated_at":"2024-01-02T04:00:00Z"}""";

        var build = Deserialize<Build>(json);

        build.Id.Should().Be(99);
        build.BuildIdentifier.Should().Be("ci-1421");
        build.CreatedAt.Should().Be(new DateTimeOffset(2024, 1, 2, 3, 4, 5, TimeSpan.Zero));
    }

    [Fact]
    public void TestCase_maps_rich_single_get_shape()
    {
        const string json = """
        {"id":123,"session_id":"abc-uuid","name":"login","state":"COMPLETE","success":1,"status_id":1,
         "status_message":"ok","created_at":"2024-01-02T03:04:05Z","completed_at":"2024-01-02T03:05:05Z",
         "duration":60,"browser":"chrome","browser_version":"121","os":"WIN10","build":"ci-1",
         "groups":["smoke","regression"],"video":"https://x/video.mp4",
         "thumbs":[{"id":1,"filename":"0001.png","url":"https://x/0001.png","custom":0}],
         "logs":{"selenium":"https://x/sel.log"},"assets_available":true,"type":"WEBDRIVER"}
        """;

        var test = Deserialize<TestCase>(json);

        test.Id.Should().Be(123);
        test.SessionId.Should().Be("abc-uuid");
        test.Success.Should().BeTrue();
        test.StatusId.Should().Be(1);
        test.Duration.Should().Be(60);
        test.Groups.Should().BeEquivalentTo("smoke", "regression");
        test.Video.Should().Be("https://x/video.mp4");
        test.Thumbs.Should().ContainSingle();
        test.Thumbs![0].Url.Should().Be("https://x/0001.png");
        test.Thumbs[0].Custom.Should().BeFalse();
        test.Logs.Should().ContainKey("selenium");
        test.AssetsAvailable.Should().BeTrue();
    }

    [Fact]
    public void TestCase_tolerates_list_shape_quirks()
    {
        // video as false, thumbs as bare strings, logs as empty array, groups as objects.
        const string json = """
        {"id":5,"session_id":"s","name":"t","state":"COMPLETE","success":false,"status_id":0,
         "video":false,"thumbs":["https://x/a.png","https://x/b.png"],"logs":[],
         "groups":[{"id":1,"name":"smoke","color":"abc"},{"id":2,"name":"nightly","color":"def"}]}
        """;

        var test = Deserialize<TestCase>(json);

        test.Success.Should().BeFalse();
        test.Video.Should().BeNull();
        test.Thumbs.Should().HaveCount(2);
        test.Thumbs![0].Url.Should().Be("https://x/a.png");
        test.Logs.Should().BeEmpty();
        test.Groups.Should().BeEquivalentTo("smoke", "nightly");
    }

    [Fact]
    public void Job_maps_status_and_preserves_extra_fields()
    {
        const string json = """
        {"status":"FINISHED","created_at":"2024-01-02T03:04:05Z","updated_at":"2024-01-02T03:10:05Z",
         "success":true,"test_ids":[1,2,3],"custom_field":"ignored"}
        """;

        var job = Deserialize<Job>(json);

        job.Status.Should().Be("FINISHED");
        job.IsComplete.Should().BeTrue();
        job.Success.Should().BeTrue();
        job.TestIds.Should().BeEquivalentTo(new long[] { 1, 2, 3 });
    }

    [Fact]
    public void User_and_keys_map_fields()
    {
        var user = Deserialize<User>("""{"first_name":"Ada","last_name":"Lovelace","email":"ada@x.io","plan":"automated","max_concurrent":5,"seconds":3600}""");
        user.FirstName.Should().Be("Ada");
        user.MaxConcurrent.Should().Be(5);
        user.Seconds.Should().Be(3600);

        var keys = Deserialize<UserKeys>("""{"key":"abc","secret":"def"}""");
        keys.Key.Should().Be("abc");
        keys.Secret.Should().Be("def");
    }
}
