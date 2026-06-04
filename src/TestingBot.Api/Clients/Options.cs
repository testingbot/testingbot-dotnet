using System.Collections.Generic;

namespace TestingBot.Api;

/// <summary>Common pagination parameters for list endpoints.</summary>
public record PageOptions
{
    /// <summary>The number of items to skip from the start of the result set.</summary>
    public int? Offset { get; init; }

    /// <summary>The number of items to return (the API caps a page at 500).</summary>
    public int? Count { get; init; }
}

/// <summary>Fields that can be omitted from test responses to reduce payload size.</summary>
[Flags]
public enum TestFieldSkip
{
    /// <summary>Omit nothing.</summary>
    None = 0,

    /// <summary>Omit the <c>logs</c> map.</summary>
    Logs = 1,

    /// <summary>Omit screenshot thumbnails.</summary>
    Thumbs = 2,

    /// <summary>Omit the rendered steps (single-test endpoint only).</summary>
    Steps = 4,
}

/// <summary>Filters and pagination for listing tests via <c>GET /v1/tests</c>.</summary>
public sealed record TestListOptions : PageOptions
{
    /// <summary>Return only tests updated at or after this time.</summary>
    public DateTimeOffset? UpdatedSince { get; init; }

    /// <summary>Return only tests that ran on this browser id (from <c>GET /v1/browsers</c>).</summary>
    public int? BrowserId { get; init; }

    /// <summary>Return only tests tagged with this group name.</summary>
    public string? Group { get; init; }

    /// <summary>Return only tests in this build (matches <c>capabilities.build</c>).</summary>
    public string? Build { get; init; }

    /// <summary>Fields to omit from each test in the response.</summary>
    public TestFieldSkip SkipFields { get; init; } = TestFieldSkip.None;
}

/// <summary>Options for fetching the tests within a build.</summary>
public sealed record BuildTestsOptions : PageOptions
{
    /// <summary>Fields to omit from each test in the response.</summary>
    public TestFieldSkip SkipFields { get; init; } = TestFieldSkip.None;
}

internal static class TestFieldSkipExtensions
{
    /// <summary>Renders the flags as the comma-separated <c>skip_fields</c> query value, or <see langword="null"/> when empty.</summary>
    public static string? ToQueryValue(this TestFieldSkip skip)
    {
        if (skip == TestFieldSkip.None)
        {
            return null;
        }

        var parts = new List<string>(3);
        if (skip.HasFlag(TestFieldSkip.Logs))
        {
            parts.Add("logs");
        }

        if (skip.HasFlag(TestFieldSkip.Thumbs))
        {
            parts.Add("thumbs");
        }

        if (skip.HasFlag(TestFieldSkip.Steps))
        {
            parts.Add("steps");
        }

        return parts.Count == 0 ? null : string.Join(',', parts);
    }
}
