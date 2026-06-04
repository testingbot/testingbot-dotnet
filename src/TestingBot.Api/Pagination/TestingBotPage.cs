using System.Collections;
using System.Collections.Generic;

namespace TestingBot.Api;

/// <summary>
/// A single page of results from a TestingBot list endpoint: the items plus their
/// <see cref="PageMeta"/>. Enumerating the page yields its items.
/// </summary>
/// <typeparam name="T">The item type.</typeparam>
public sealed class TestingBotPage<T> : IReadOnlyList<T>
{
    private readonly IReadOnlyList<T> _data;

    /// <summary>Initializes a new instance of the <see cref="TestingBotPage{T}"/> class.</summary>
    /// <param name="data">The items in this page.</param>
    /// <param name="meta">The pagination metadata for this page.</param>
    public TestingBotPage(IReadOnlyList<T> data, PageMeta meta)
    {
        this._data = data ?? [];
        Meta = meta ?? new PageMeta();
    }

    /// <summary>The items in this page.</summary>
    public IReadOnlyList<T> Data => this._data;

    /// <summary>The pagination metadata for this page.</summary>
    public PageMeta Meta { get; }

    /// <inheritdoc />
    public int Count => this._data.Count;

    /// <inheritdoc />
    public T this[int index] => this._data[index];

    /// <inheritdoc />
    public IEnumerator<T> GetEnumerator() => this._data.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
