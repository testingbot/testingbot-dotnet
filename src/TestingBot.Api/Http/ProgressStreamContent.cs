using System.IO;
using System.Net;
using System.Net.Http;

namespace TestingBot.Api.Http;

/// <summary>
/// Streams a source stream as request content without buffering it in memory, reporting the
/// cumulative number of bytes sent to an optional <see cref="IProgress{T}"/>. The source stream is
/// not disposed by this content; the caller retains ownership.
/// </summary>
internal sealed class ProgressStreamContent : HttpContent
{
    private const int DefaultBufferSize = 81920;

    private readonly Stream _source;
    private readonly IProgress<long>? _progress;
    private readonly int _bufferSize;

    public ProgressStreamContent(Stream source, IProgress<long>? progress = null, int bufferSize = DefaultBufferSize)
    {
        this._source = source ?? throw new ArgumentNullException(nameof(source));
        this._progress = progress;
        this._bufferSize = bufferSize;
    }

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context)
        => SerializeAsync(stream, CancellationToken.None);

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context, CancellationToken cancellationToken)
        => SerializeAsync(stream, cancellationToken);

    protected override bool TryComputeLength(out long length)
    {
        if (this._source.CanSeek)
        {
            length = this._source.Length;
            return true;
        }

        length = 0;
        return false;
    }

    private async Task SerializeAsync(Stream destination, CancellationToken cancellationToken)
    {
        var buffer = new byte[this._bufferSize];
        long total = 0;
        int read;
        while ((read = await this._source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) > 0)
        {
            await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
            total += read;
            this._progress?.Report(total);
        }
    }
}
