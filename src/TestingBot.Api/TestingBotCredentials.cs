using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace TestingBot.Api;

/// <summary>
/// Resolves TestingBot API credentials from, in order of precedence:
/// <list type="number">
/// <item>explicit values passed by the caller;</item>
/// <item>a <c>~/.testingbot</c> file containing <c>key:secret</c>;</item>
/// <item>the <c>TESTINGBOT_KEY</c> / <c>TESTINGBOT_SECRET</c> environment variables;</item>
/// <item>the <c>TB_KEY</c> / <c>TB_SECRET</c> environment variables.</item>
/// </list>
/// This mirrors the credential resolution used by the other official TestingBot SDKs.
/// </summary>
public static class TestingBotCredentials
{
    /// <summary>
    /// Resolves credentials, throwing <see cref="TestingBotConfigurationException"/> when none are found.
    /// </summary>
    /// <param name="key">An explicit key, or <see langword="null"/> to fall back to other sources.</param>
    /// <param name="secret">An explicit secret, or <see langword="null"/> to fall back to other sources.</param>
    /// <returns>The resolved key and secret.</returns>
    public static (string Key, string Secret) Resolve(string? key = null, string? secret = null)
    {
        if (TryResolve(key, secret, out var resolvedKey, out var resolvedSecret))
        {
            return (resolvedKey, resolvedSecret);
        }

        throw new TestingBotConfigurationException(
            "Could not resolve TestingBot credentials. Provide them explicitly, set TESTINGBOT_KEY/" +
            "TESTINGBOT_SECRET or TB_KEY/TB_SECRET, or create a ~/.testingbot file containing 'key:secret'.");
    }

    /// <summary>Attempts to resolve credentials without throwing.</summary>
    /// <param name="key">An explicit key, or <see langword="null"/>.</param>
    /// <param name="secret">An explicit secret, or <see langword="null"/>.</param>
    /// <param name="resolvedKey">The resolved key, when the method returns <see langword="true"/>.</param>
    /// <param name="resolvedSecret">The resolved secret, when the method returns <see langword="true"/>.</param>
    /// <returns><see langword="true"/> if both a key and secret were resolved; otherwise <see langword="false"/>.</returns>
    public static bool TryResolve(
        string? key,
        string? secret,
        [NotNullWhen(true)] out string? resolvedKey,
        [NotNullWhen(true)] out string? resolvedSecret)
        => TryResolveCore(key, secret, GetHomeDirectory(), Environment.GetEnvironmentVariable, out resolvedKey, out resolvedSecret);

    /// <summary>Testable core: resolution with an injectable home directory and environment reader.</summary>
    internal static bool TryResolveCore(
        string? key,
        string? secret,
        string? homeDirectory,
        Func<string, string?> getEnvironmentVariable,
        [NotNullWhen(true)] out string? resolvedKey,
        [NotNullWhen(true)] out string? resolvedSecret)
    {
        if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(secret))
        {
            resolvedKey = key;
            resolvedSecret = secret;
            return true;
        }

        if (TryReadConfigFile(homeDirectory, out var fileKey, out var fileSecret))
        {
            resolvedKey = fileKey;
            resolvedSecret = fileSecret;
            return true;
        }

        if (TryReadEnv(getEnvironmentVariable, "TESTINGBOT_KEY", "TESTINGBOT_SECRET", out var envKey, out var envSecret) ||
            TryReadEnv(getEnvironmentVariable, "TB_KEY", "TB_SECRET", out envKey, out envSecret))
        {
            resolvedKey = envKey;
            resolvedSecret = envSecret;
            return true;
        }

        resolvedKey = null;
        resolvedSecret = null;
        return false;
    }

    private static string? GetHomeDirectory()
    {
        try
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }
        catch (Exception)
        {
            return null;
        }
    }

    private static bool TryReadEnv(
        Func<string, string?> getEnvironmentVariable,
        string keyVar,
        string secretVar,
        [NotNullWhen(true)] out string? key,
        [NotNullWhen(true)] out string? secret)
    {
        var k = getEnvironmentVariable(keyVar);
        var s = getEnvironmentVariable(secretVar);
        if (!string.IsNullOrWhiteSpace(k) && !string.IsNullOrWhiteSpace(s))
        {
            key = k;
            secret = s;
            return true;
        }

        key = null;
        secret = null;
        return false;
    }

    private static bool TryReadConfigFile(
        string? homeDirectory,
        [NotNullWhen(true)] out string? key,
        [NotNullWhen(true)] out string? secret)
    {
        key = null;
        secret = null;

        if (string.IsNullOrEmpty(homeDirectory))
        {
            return false;
        }

        var path = Path.Combine(homeDirectory, ".testingbot");
        if (!File.Exists(path))
        {
            return false;
        }

        string firstLine;
        try
        {
            firstLine = File.ReadLines(path).FirstOrDefault(static line => !string.IsNullOrWhiteSpace(line)) ?? string.Empty;
        }
        catch (Exception)
        {
            return false;
        }

        var separator = firstLine.IndexOf(':', StringComparison.Ordinal);
        if (separator <= 0 || separator >= firstLine.Length - 1)
        {
            return false;
        }

        var k = firstLine[..separator].Trim();
        var s = firstLine[(separator + 1)..].Trim();
        if (string.IsNullOrEmpty(k) || string.IsNullOrEmpty(s))
        {
            return false;
        }

        key = k;
        secret = s;
        return true;
    }
}
