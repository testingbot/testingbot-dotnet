using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;

namespace TestingBot.Api.Http;

/// <summary>
/// Builds <c>application/x-www-form-urlencoded</c> content using the Rails-style nested key
/// convention the TestingBot API expects (for example <c>test[name]</c>, <c>suite[cron]</c>).
/// Booleans are sent as <c>1</c>/<c>0</c>. Fields with a <see langword="null"/> value are omitted.
/// </summary>
internal sealed class FormContentBuilder
{
    private readonly List<KeyValuePair<string, string>> _fields = [];

    public bool IsEmpty => this._fields.Count == 0;

    public FormContentBuilder Add(string name, string? value)
    {
        if (value is not null)
        {
            this._fields.Add(new KeyValuePair<string, string>(name, value));
        }

        return this;
    }

    public FormContentBuilder Add(string name, bool? value)
        => value.HasValue ? Add(name, value.Value ? "1" : "0") : this;

    public FormContentBuilder Add(string name, int? value)
        => value.HasValue ? Add(name, value.Value.ToString(CultureInfo.InvariantCulture)) : this;

    public FormContentBuilder Add(string name, long? value)
        => value.HasValue ? Add(name, value.Value.ToString(CultureInfo.InvariantCulture)) : this;

    /// <summary>Adds a Rails-nested field, e.g. <c>Add("test", "name", value)</c> → <c>test[name]</c>.</summary>
    public FormContentBuilder Add(string container, string field, string? value)
        => Add($"{container}[{field}]", value);

    public FormContentBuilder Add(string container, string field, bool? value)
        => Add($"{container}[{field}]", value);

    public FormContentBuilder Add(string container, string field, int? value)
        => Add($"{container}[{field}]", value);

    public FormUrlEncodedContent Build() => new(this._fields);
}
