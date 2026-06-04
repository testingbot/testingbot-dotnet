using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TestingBot.Api.Http;

namespace TestingBot.Api;

/// <summary>Shared base for resource clients: holds the connection and common pagination helpers.</summary>
internal abstract class ResourceClient
{
    protected ResourceClient(TestingBotConnection connection)
    {
        Connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }

    protected TestingBotConnection Connection { get; }

    /// <summary>Clamps a requested page size to the API's accepted range, defaulting from options.</summary>
    protected int NormalizePageSize(int? count)
        => Math.Clamp(count ?? Connection.Options.DefaultPageSize, 1, 500);

    /// <summary>
    /// Iterates every item across all pages of a list endpoint, fetching pages lazily.
    /// Pages are walked by offset until a short (or empty) page is returned.
    /// </summary>
    protected async IAsyncEnumerable<T> PaginateAsync<T>(
        Func<int, int, CancellationToken, Task<TestingBotPage<T>>> fetchPage,
        int? pageSize,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var size = NormalizePageSize(pageSize);
        var offset = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var page = await fetchPage(offset, size, cancellationToken).ConfigureAwait(false);
            if (page.Data.Count == 0)
            {
                yield break;
            }

            foreach (var item in page.Data)
            {
                yield return item;
            }

            if (page.Data.Count < size)
            {
                yield break;
            }

            offset += page.Data.Count;
        }
    }

    /// <summary>Escapes a path identifier while preserving any <c>/</c> separators (build identifiers may contain slashes).</summary>
    protected static string EscapePathPreservingSlash(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        return string.Join('/', value.Split('/').Select(Uri.EscapeDataString));
    }
}
