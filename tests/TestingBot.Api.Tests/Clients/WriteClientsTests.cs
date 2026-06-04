using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using RichardSzalay.MockHttp;
using TestingBot.Api.Tests.TestSupport;

namespace TestingBot.Api.Tests.Clients;

public class WriteClientsTests
{
    private const string BaseUrl = TestConnectionFactory.BaseUrl;

    [Fact]
    public async Task Tests_Update_sends_rails_form_and_success_as_one()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Put, BaseUrl + "tests/123")
            .WithFormData("test[name]", "login")
            .WithFormData("test[success]", "1")
            .WithFormData("groups", "smoke,nightly")
            .Respond("application/json", "{\"success\":true}");

        var ok = await new TestsClient(connection).UpdateAsync("123", new TestUpdate
        {
            Name = "login",
            Success = true,
            Groups = ["smoke", "nightly"],
        });

        ok.Should().BeTrue();
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Tests_Stop_and_Delete()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Put, BaseUrl + "tests/s-1/stop").Respond("application/json", "{\"success\":true}");
        mock.When(HttpMethod.Delete, BaseUrl + "tests/s-1").Respond("application/json", "{\"success\":true}");

        var client = new TestsClient(connection);
        (await client.StopAsync("s-1")).Should().BeTrue();
        (await client.DeleteAsync("s-1")).Should().BeTrue();
    }

    [Fact]
    public async Task User_Update_sends_user_form()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Put, BaseUrl + "user")
            .WithFormData("user[first_name]", "Ada")
            .Respond("application/json", "{\"success\":true}");

        var ok = await new UserClient(connection).UpdateAsync(new UserUpdate { FirstName = "Ada" });

        ok.Should().BeTrue();
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Tunnels_List_Create_and_Stop()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Get, BaseUrl + "tunnel/list").Respond("application/json", "[{\"id\":1,\"state\":\"READY\"}]");
        mock.When(HttpMethod.Post, BaseUrl + "tunnel/create").Respond("application/json", "{\"id\":2,\"state\":\"BOOTING\"}");
        mock.When(HttpMethod.Delete, BaseUrl + "tunnel/2").Respond(HttpStatusCode.OK, "application/json", "{}");

        var client = new TunnelsClient(connection);
        (await client.ListAsync()).Should().ContainSingle();
        (await client.CreateAsync()).Id.Should().Be(2);
        (await client.StopAsync(2)).Should().BeTrue();
    }

    [Fact]
    public async Task Tunnels_IsAlive_returns_false_on_error()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Get, BaseUrl + "tunnel/isalive-check").Respond(HttpStatusCode.InternalServerError, "application/json", "{\"error\":\"down\"}");

        (await new TunnelsClient(connection).IsAliveAsync()).Should().BeFalse();
    }

    [Fact]
    public async Task Team_GetConcurrency_unwraps_envelope()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Get, BaseUrl + "team-management")
            .Respond("application/json", "{\"concurrency\":{\"allowed\":{\"vms\":5,\"physical\":2},\"current\":{\"vms\":1,\"physical\":0}}}");

        var concurrency = await new TeamClient(connection).GetConcurrencyAsync();

        concurrency.Allowed!.Vms.Should().Be(5);
        concurrency.Current!.Physical.Should().Be(0);
    }

    [Fact]
    public async Task Team_CreateUser_posts_form_and_returns_member()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Post, BaseUrl + "team-management/users")
            .WithFormData("email", "new@x.io")
            .WithFormData("concurrencyPhysical", "2")
            .Respond("application/json", "{\"id\":10,\"email\":\"new@x.io\"}");

        var member = await new TeamClient(connection).CreateUserAsync(new TeamMemberCreate
        {
            Email = "new@x.io",
            Password = "pw",
            ConcurrencyPhysical = 2,
        });

        member.Id.Should().Be(10);
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Team_GetUserClientKey_extracts_field()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Get, BaseUrl + "team-management/users/3/client-key")
            .Respond("application/json", "{\"client_key\":\"ck-123\"}");

        (await new TeamClient(connection).GetUserClientKeyAsync(3)).Should().Be("ck-123");
    }

    [Fact]
    public async Task CodelessTests_Create_returns_new_id()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Post, BaseUrl + "lab")
            .WithFormData("test[name]", "homepage")
            .WithFormData("test[url]", "https://x.io")
            .Respond("application/json", "{\"success\":true,\"lab_test_id\":77}");

        var id = await new CodelessTestsClient(connection).CreateAsync(new CodelessTestCreate { Name = "homepage", Url = "https://x.io" });

        id.Should().Be(77);
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task CodelessTests_Create_throws_on_success_false()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Post, BaseUrl + "lab")
            .Respond("application/json", "{\"success\":false,\"errors\":\"name is required\"}");

        var act = () => new CodelessTestsClient(connection).CreateAsync(new CodelessTestCreate { Name = "x" });

        await act.Should().ThrowAsync<TestingBotValidationException>();
    }

    [Fact]
    public async Task CodelessTests_Trigger_returns_job_id()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Post, BaseUrl + "lab/5/trigger").Respond("application/json", "{\"success\":true,\"job_id\":555}");

        var result = await new CodelessTestsClient(connection).TriggerAsync(5);

        result.Success.Should().BeTrue();
        result.JobId.Should().Be(555);
    }

    [Fact]
    public async Task CodelessTests_AddAlert_sends_kind_level_content()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Post, BaseUrl + "lab/5/alert")
            .WithFormData("kind", "EMAIL")
            .WithFormData("level", "IMMEDIATELY")
            .WithFormData("content", "a@b.io")
            .Respond("application/json", "{\"success\":true}");

        var ok = await new CodelessTestsClient(connection).AddAlertAsync(5, new CodelessAlertInput
        {
            Kind = AlertKind.Email,
            Level = AlertLevel.Immediately,
            Content = "a@b.io",
        });

        ok.Should().BeTrue();
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task CodelessTests_SetSteps_posts_json_body()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Post, BaseUrl + "lab/5/steps")
            .WithPartialContent("\"cmd\":\"open\"")
            .Respond("application/json", "{\"success\":true}");

        var ok = await new CodelessTestsClient(connection).SetStepsAsync(5,
        [
            new CodelessStepInput { Order = 0, Command = "open", Locator = "/", Value = null },
        ]);

        ok.Should().BeTrue();
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task CodelessSuites_Create_and_Trigger()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Post, BaseUrl + "labsuites").Respond("application/json", "{\"success\":true,\"suite_id\":12}");
        mock.When(HttpMethod.Post, BaseUrl + "labsuites/12/trigger").Respond("application/json", "{\"success\":true,\"job_id\":900}");

        var client = new CodelessSuitesClient(connection);
        (await client.CreateAsync(new CodelessSuiteCreate { Name = "nightly" })).Should().Be(12);
        (await client.TriggerAsync(12)).JobId.Should().Be(900);
    }

    [Fact]
    public async Task Screenshots_Capture_posts_json_and_returns_batch()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Post, BaseUrl + "screenshots")
            .WithPartialContent("\"resolution\":\"1920x1080\"")
            .WithPartialContent("\"browsers\":[1,2]")
            .Respond("application/json", "{\"id\":9,\"url\":\"https://x.io\",\"resolution\":\"1920x1080\"}");

        var batch = await new ScreenshotsClient(connection).CaptureAsync(new ScreenshotRequest
        {
            Url = "https://x.io",
            Resolution = "1920x1080",
            BrowserIds = [1, 2],
        });

        batch.Id.Should().Be(9);
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Screenshots_Get_sends_excludeIds()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Get, BaseUrl + "screenshots/9")
            .WithExactQueryString("excludeIds=3,4")
            .Respond("application/json", "{\"id\":9,\"state\":\"processing\",\"screenshots\":[]}");

        var batch = await new ScreenshotsClient(connection).GetAsync(9, [3, 4]);

        batch.State.Should().Be("processing");
        mock.VerifyNoOutstandingExpectation();
    }
}
