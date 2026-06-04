using System.IO;
using System.Net.Http;
using System.Text;
using RichardSzalay.MockHttp;
using TestingBot.Api.Tests.TestSupport;

namespace TestingBot.Api.Tests.Clients;

public class StorageClientTests
{
    private const string BaseUrl = TestConnectionFactory.BaseUrl;

    [Fact]
    public async Task Upload_streams_multipart_and_returns_app_url()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Post, BaseUrl + "storage")
            .With(request => request.Content is MultipartFormDataContent)
            .Respond("application/json", "{\"app_url\":\"tb://abc123\"}");

        var bytes = Encoding.UTF8.GetBytes(new string('x', 200_000));
        using var stream = new MemoryStream(bytes);

        var result = await new StorageClient(connection).UploadAsync(stream, "app.apk");

        result.AppUrl.Should().Be("tb://abc123");
        result.AppKey.Should().Be("abc123");
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task UploadFromUrl_sends_url_form()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Post, BaseUrl + "storage")
            .WithFormData("url", "https://example.test/app.apk")
            .Respond("application/json", "{\"app_url\":\"tb://def456\"}");

        var result = await new StorageClient(connection).UploadFromUrlAsync(new Uri("https://example.test/app.apk"));

        result.AppKey.Should().Be("def456");
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Get_strips_tb_prefix()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Get, BaseUrl + "storage/abc123")
            .Respond("application/json", "{\"id\":1,\"app_url\":\"tb://abc123\",\"type\":\"apk\",\"state\":\"READY\"}");

        var file = await new StorageClient(connection).GetAsync("tb://abc123");

        file.Type.Should().Be("apk");
        file.State.Should().Be("READY");
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task List_returns_page()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Get, BaseUrl + "storage")
            .Respond("application/json", "{\"data\":[{\"id\":1,\"app_url\":\"tb://a\"},{\"id\":2,\"app_url\":\"tb://b\"}],\"meta\":{\"offset\":0,\"count\":2,\"total\":2}}");

        var page = await new StorageClient(connection).ListAsync();

        page.Data.Should().HaveCount(2);
        page.Data[0].AppKey.Should().Be("a");
    }

    [Fact]
    public async Task Replace_posts_to_appkey_path()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Post, BaseUrl + "storage/abc123")
            .With(request => request.Content is MultipartFormDataContent)
            .Respond("application/json", "{\"app_url\":\"tb://abc123\"}");

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("new"));
        var result = await new StorageClient(connection).ReplaceAsync("tb://abc123", stream, "app.apk");

        result.AppKey.Should().Be("abc123");
        mock.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task Delete_returns_success()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.When(HttpMethod.Delete, BaseUrl + "storage/abc123").Respond("application/json", "{\"success\":true}");

        (await new StorageClient(connection).DeleteAsync("abc123")).Should().BeTrue();
    }

    [Fact]
    public async Task Upload_from_FileInfo_reads_disk()
    {
        var (connection, mock) = TestConnectionFactory.Create();
        mock.Expect(HttpMethod.Post, BaseUrl + "storage")
            .With(request => request.Content is MultipartFormDataContent)
            .Respond("application/json", "{\"app_url\":\"tb://fromfile\"}");

        var path = Path.Combine(Path.GetTempPath(), "tb-upload-" + Guid.NewGuid().ToString("N") + ".apk");
        await File.WriteAllTextAsync(path, "binary-content");
        try
        {
            var result = await new StorageClient(connection).UploadAsync(new FileInfo(path));
            result.AppKey.Should().Be("fromfile");
        }
        finally
        {
            File.Delete(path);
        }

        mock.VerifyNoOutstandingExpectation();
    }
}
