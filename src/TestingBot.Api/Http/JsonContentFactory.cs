using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;

namespace TestingBot.Api.Http;

/// <summary>Builds <c>application/json</c> request bodies without reflection, AOT-safely.</summary>
internal static class JsonContentFactory
{
    public static HttpContent Create(Action<Utf8JsonWriter> write)
    {
        ArgumentNullException.ThrowIfNull(write);

        using var buffer = new MemoryStream();
        using (var writer = new Utf8JsonWriter(buffer))
        {
            write(writer);
        }

        var content = new ByteArrayContent(buffer.ToArray());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        return content;
    }
}
