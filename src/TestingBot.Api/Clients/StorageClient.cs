using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using TestingBot.Api.Http;
using TestingBot.Api.Models;

namespace TestingBot.Api;

/// <summary>
/// Operations for the TestingBot app/binary store. Uploads are streamed (not buffered in memory),
/// support progress reporting, and use a longer timeout than ordinary API calls.
/// </summary>
public interface IStorageClient
{
    /// <summary>Lists uploaded apps, newest first (<c>GET /v1/storage</c>).</summary>
    Task<TestingBotPage<StorageFile>> ListAsync(PageOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lazily iterates every uploaded app across all pages.</summary>
    IAsyncEnumerable<StorageFile> ListAllAsync(int? pageSize = null, CancellationToken cancellationToken = default);

    /// <summary>Gets metadata for one uploaded app by app key or <c>tb://</c> URL (<c>GET /v1/storage/:appkey</c>).</summary>
    Task<StorageFile> GetAsync(string appUrlOrKey, CancellationToken cancellationToken = default);

    /// <summary>Uploads an app from a stream (<c>POST /v1/storage</c>).</summary>
    /// <param name="content">The binary content. The stream is read but not disposed.</param>
    /// <param name="fileName">The file name to associate with the upload (e.g. <c>app.apk</c>).</param>
    /// <param name="progress">An optional receiver of cumulative bytes sent.</param>
    /// <param name="cancellationToken">A token to cancel the upload.</param>
    /// <returns>The uploaded app (its <c>app_url</c> is populated; call <see cref="GetAsync"/> for full metadata).</returns>
    Task<StorageFile> UploadAsync(Stream content, string fileName, IProgress<long>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>Uploads an app from a file on disk (<c>POST /v1/storage</c>).</summary>
    Task<StorageFile> UploadAsync(FileInfo file, IProgress<long>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>Uploads an app by having TestingBot fetch a public URL (<c>POST /v1/storage</c>).</summary>
    Task<StorageFile> UploadFromUrlAsync(Uri url, CancellationToken cancellationToken = default);

    /// <summary>Replaces the binary behind an existing app key from a stream (<c>POST /v1/storage/:appkey</c>).</summary>
    Task<StorageFile> ReplaceAsync(string appKey, Stream content, string fileName, IProgress<long>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>Replaces the binary behind an existing app key from a file on disk.</summary>
    Task<StorageFile> ReplaceAsync(string appKey, FileInfo file, IProgress<long>? progress = null, CancellationToken cancellationToken = default);

    /// <summary>Replaces the binary behind an existing app key by fetching a public URL.</summary>
    Task<StorageFile> ReplaceFromUrlAsync(string appKey, Uri url, CancellationToken cancellationToken = default);

    /// <summary>Deletes an uploaded app by numeric id or app key (<c>DELETE /v1/storage/:id</c>).</summary>
    Task<bool> DeleteAsync(string idOrAppKey, CancellationToken cancellationToken = default);
}

internal sealed class StorageClient(TestingBotConnection connection) : ResourceClient(connection), IStorageClient
{
    public Task<TestingBotPage<StorageFile>> ListAsync(PageOptions? options = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryString().Add("offset", options?.Offset).Add("count", options?.Count);
        return Connection.GetPageAsync<StorageFile>("storage", query, cancellationToken);
    }

    public IAsyncEnumerable<StorageFile> ListAllAsync(int? pageSize = null, CancellationToken cancellationToken = default)
        => PaginateAsync((offset, size, token) => ListAsync(new PageOptions { Offset = offset, Count = size }, token), pageSize, cancellationToken);

    public Task<StorageFile> GetAsync(string appUrlOrKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appUrlOrKey);
        return Connection.GetAsync<StorageFile>($"storage/{Uri.EscapeDataString(NormalizeAppKey(appUrlOrKey))}", query: null, cancellationToken);
    }

    public Task<StorageFile> UploadAsync(Stream content, string fileName, IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        return UploadMultipartAsync("storage", content, fileName, progress, cancellationToken);
    }

    public async Task<StorageFile> UploadAsync(FileInfo file, IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        var stream = file.OpenRead();
        await using (stream.ConfigureAwait(false))
        {
            return await UploadMultipartAsync("storage", stream, file.Name, progress, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task<StorageFile> UploadFromUrlAsync(Uri url, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(url);
        var form = new FormContentBuilder().Add("url", url.ToString());
        return Connection.SendForAsync<StorageFile>(HttpMethod.Post, "storage", form.Build(), query: null, isUpload: true, cancellationToken);
    }

    public Task<StorageFile> ReplaceAsync(string appKey, Stream content, string fileName, IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appKey);
        ArgumentNullException.ThrowIfNull(content);
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        return UploadMultipartAsync($"storage/{Uri.EscapeDataString(NormalizeAppKey(appKey))}", content, fileName, progress, cancellationToken);
    }

    public async Task<StorageFile> ReplaceAsync(string appKey, FileInfo file, IProgress<long>? progress = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);
        var stream = file.OpenRead();
        await using (stream.ConfigureAwait(false))
        {
            return await ReplaceAsync(appKey, stream, file.Name, progress, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task<StorageFile> ReplaceFromUrlAsync(string appKey, Uri url, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appKey);
        ArgumentNullException.ThrowIfNull(url);
        var form = new FormContentBuilder().Add("url", url.ToString());
        return Connection.SendForAsync<StorageFile>(HttpMethod.Post, $"storage/{Uri.EscapeDataString(NormalizeAppKey(appKey))}", form.Build(), query: null, isUpload: true, cancellationToken);
    }

    public Task<bool> DeleteAsync(string idOrAppKey, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(idOrAppKey);
        return Connection.SendAckAsync(HttpMethod.Delete, $"storage/{Uri.EscapeDataString(NormalizeAppKey(idOrAppKey))}", content: null, query: null, cancellationToken);
    }

    private async Task<StorageFile> UploadMultipartAsync(string path, Stream content, string fileName, IProgress<long>? progress, CancellationToken cancellationToken)
    {
        var filePart = new ProgressStreamContent(content, progress);
        filePart.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

        using var multipart = new MultipartFormDataContent { { filePart, "file", fileName } };
        return await Connection.SendForAsync<StorageFile>(HttpMethod.Post, path, multipart, query: null, isUpload: true, cancellationToken).ConfigureAwait(false);
    }

    private static string NormalizeAppKey(string value)
        => value.StartsWith("tb://", StringComparison.OrdinalIgnoreCase) ? value[5..] : value;
}
