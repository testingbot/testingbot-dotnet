using System.Net.Http;
using RichardSzalay.MockHttp;
using TestingBot.Api.Tests.TestSupport;

namespace TestingBot.Api.Tests.Clients;

public class ReadClientsTests
{
    private const string BaseUrl = TestConnectionFactory.BaseUrl;

    [Fact]
    public async Task Configuration_GetIpRanges_returns_bare_array()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Get, BaseUrl + "configuration/ip-ranges")
            .Respond("application/json", "[\"1.2.3.4\",\"5.6.7.8\"]");

        var ranges = await new ConfigurationClient(connection).GetIpRangesAsync();

        ranges.Should().BeEquivalentTo("1.2.3.4", "5.6.7.8");
    }

    [Fact]
    public async Task Browsers_List_sends_type_filter()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Get, BaseUrl + "browsers")
            .WithExactQueryString("type=rc")
            .Respond("application/json", "[{\"name\":\"firefox\",\"browser_id\":1}]");

        var browsers = await new BrowsersClient(connection).ListAsync(BrowserType.Rc);

        browsers.Should().ContainSingle();
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Devices_List_maps_platform_filter()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Get, BaseUrl + "devices")
            .WithExactQueryString("platform=REAL_IOS")
            .Respond("application/json", "[{\"id\":1,\"available\":true}]");

        var devices = await new DevicesClient(connection).ListAsync(DevicePlatform.RealIos);

        devices.Should().ContainSingle();
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Devices_Get_hits_id_route()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Get, BaseUrl + "devices/42")
            .Respond("application/json", "{\"id\":42,\"name\":\"Pixel 8\",\"available\":false}");

        var device = await new DevicesClient(connection).GetAsync(42);

        device.Id.Should().Be(42);
        device.Name.Should().Be("Pixel 8");
    }

    [Fact]
    public async Task User_Get_and_Keys()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Get, BaseUrl + "user").Respond("application/json", "{\"email\":\"a@b.c\"}");
        mock.When(HttpMethod.Get, BaseUrl + "user/keys").Respond("application/json", "{\"key\":\"k\",\"secret\":\"s\"}");

        var client = new UserClient(connection);
        (await client.GetAsync()).Email.Should().Be("a@b.c");
        (await client.GetKeysAsync()).Key.Should().Be("k");
    }

    [Fact]
    public async Task Tests_Get_always_skips_steps()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Get, BaseUrl + "tests/sess-1")
            .WithExactQueryString("skip_fields=thumbs,steps")
            .Respond("application/json", "{\"id\":1,\"session_id\":\"sess-1\"}");

        await new TestsClient(connection).GetAsync("sess-1", TestFieldSkip.Thumbs);

        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Tests_List_builds_filter_query()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Get, BaseUrl + "tests")
            .WithQueryString(new Dictionary<string, string>
            {
                ["offset"] = "20",
                ["count"] = "10",
                ["build"] = "ci-1",
                ["browser_id"] = "3",
            })
            .Respond("application/json", "{\"data\":[],\"meta\":{\"offset\":20,\"count\":0,\"total\":40}}");

        var page = await new TestsClient(connection).ListAsync(new TestListOptions
        {
            Offset = 20,
            Count = 10,
            Build = "ci-1",
            BrowserId = 3,
        });

        page.Meta.Total.Should().Be(40);
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Tests_ListAll_auto_paginates()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Get, BaseUrl + "tests")
            .WithExactQueryString("offset=0&count=2")
            .Respond("application/json", "{\"data\":[{\"id\":1},{\"id\":2}],\"meta\":{\"offset\":0,\"count\":2,\"total\":3}}");
        mock.When(HttpMethod.Get, BaseUrl + "tests")
            .WithExactQueryString("offset=2&count=2")
            .Respond("application/json", "{\"data\":[{\"id\":3}],\"meta\":{\"offset\":2,\"count\":1,\"total\":3}}");

        var ids = new List<long>();
        await foreach (var test in new TestsClient(connection).ListAllAsync(new TestListOptions { Count = 2 }))
        {
            ids.Add(test.Id);
        }

        ids.Should().Equal(1, 2, 3);
    }

    [Fact]
    public async Task Builds_GetTests_returns_page()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Get, BaseUrl + "builds/ci-1")
            .Respond("application/json", "{\"data\":[{\"id\":1,\"session_id\":\"s\"}],\"meta\":{\"offset\":0,\"count\":1,\"total\":1}}");

        var page = await new BuildsClient(connection).GetTestsAsync("ci-1");

        page.Data.Should().ContainSingle();
        page.Data[0].SessionId.Should().Be("s");
    }

    [Fact]
    public async Task Builds_Delete_returns_success()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Delete, BaseUrl + "builds/99").Respond("application/json", "{\"success\":true}");

        var deleted = await new BuildsClient(connection).DeleteAsync("99");

        deleted.Should().BeTrue();
    }

    [Fact]
    public async Task Jobs_WaitForCompletion_polls_until_terminal()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        var calls = 0;
        mock.When(HttpMethod.Get, BaseUrl + "jobs/7").Respond(_ =>
        {
            calls++;
            var status = calls < 2 ? "RUNNING" : "FINISHED";
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent($"{{\"status\":\"{status}\",\"success\":true}}"),
            };
        });

        var job = await new JobsClient(connection).WaitForCompletionAsync(7, pollInterval: TimeSpan.FromMilliseconds(1));

        job.Status.Should().Be("FINISHED");
        calls.Should().BeGreaterThanOrEqualTo(2);
    }
}
