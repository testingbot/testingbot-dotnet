using System.Collections.Generic;
using System.IO;
using System.Text;
using TestingBot.Api.Http;

namespace TestingBot.Api.Tests.Http;

public class ProgressStreamContentTests
{
    private sealed class SyncProgress : IProgress<long>
    {
        public List<long> Values { get; } = [];

        public void Report(long value) => Values.Add(value);
    }

    [Fact]
    public async Task Reports_cumulative_bytes_and_ends_at_total_length()
    {
        var bytes = Encoding.UTF8.GetBytes(new string('x', 200_000));
        using var source = new MemoryStream(bytes);
        var progress = new SyncProgress();
        var content = new ProgressStreamContent(source, progress);

        using var destination = new MemoryStream();
        await content.CopyToAsync(destination);

        destination.Length.Should().Be(bytes.Length);
        progress.Values.Should().NotBeEmpty();
        progress.Values[^1].Should().Be(bytes.Length);
        progress.Values.Should().BeInAscendingOrder();
    }

    [Fact]
    public void Computes_length_for_seekable_stream()
    {
        using var source = new MemoryStream(new byte[1024]);
        var content = new ProgressStreamContent(source);

        content.Headers.ContentLength.Should().Be(1024);
    }

    [Fact]
    public async Task Does_not_dispose_the_source_stream()
    {
        using var source = new MemoryStream(Encoding.UTF8.GetBytes("data"));
        var content = new ProgressStreamContent(source);

        using var destination = new MemoryStream();
        await content.CopyToAsync(destination);

        // The caller retains ownership: the stream must still be usable.
        source.CanRead.Should().BeTrue();
    }
}
