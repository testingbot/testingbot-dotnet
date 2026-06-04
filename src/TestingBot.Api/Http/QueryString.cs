using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace TestingBot.Api.Http;

/// <summary>
/// Builds a URL query string, URL-encoding values and skipping any whose value is
/// <see langword="null"/>. Booleans are rendered as <c>true</c>/<c>false</c>.
/// </summary>
internal sealed class QueryString
{
    private readonly List<KeyValuePair<string, string>> _pairs = [];

    public bool IsEmpty => this._pairs.Count == 0;

    public QueryString Add(string name, string? value)
    {
        if (value is not null)
        {
            this._pairs.Add(new KeyValuePair<string, string>(name, value));
        }

        return this;
    }

    public QueryString Add(string name, int? value)
        => value.HasValue ? Add(name, value.Value.ToString(CultureInfo.InvariantCulture)) : this;

    public QueryString Add(string name, long? value)
        => value.HasValue ? Add(name, value.Value.ToString(CultureInfo.InvariantCulture)) : this;

    public QueryString Add(string name, bool? value)
        => value.HasValue ? Add(name, value.Value ? "true" : "false") : this;

    /// <summary>Renders the query string without a leading <c>?</c>.</summary>
    public override string ToString()
    {
        if (IsEmpty)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        for (var i = 0; i < this._pairs.Count; i++)
        {
            if (i > 0)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(this._pairs[i].Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(this._pairs[i].Value));
        }

        return builder.ToString();
    }
}
