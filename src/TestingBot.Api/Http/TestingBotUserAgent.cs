using System.Reflection;
using System.Runtime.InteropServices;

namespace TestingBot.Api.Http;

/// <summary>Builds the default <c>User-Agent</c> string the SDK sends with every request.</summary>
internal static class TestingBotUserAgent
{
    /// <summary>Returns <paramref name="custom"/> when provided, otherwise a descriptive default.</summary>
    public static string Resolve(string? custom)
    {
        if (!string.IsNullOrWhiteSpace(custom))
        {
            return custom;
        }

        var version = typeof(TestingBotUserAgent).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? typeof(TestingBotUserAgent).Assembly.GetName().Version?.ToString()
            ?? "0.0.0";

        // Strip any build metadata suffix (e.g. "1.0.0+abc1234").
        var plus = version.IndexOf('+', StringComparison.Ordinal);
        if (plus >= 0)
        {
            version = version[..plus];
        }

        return $"testingbot-dotnet/{version} ({RuntimeInformation.OSDescription}; {RuntimeInformation.FrameworkDescription})";
    }
}
