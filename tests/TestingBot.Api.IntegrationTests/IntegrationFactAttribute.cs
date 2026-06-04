using TestingBot.Api;

namespace TestingBot.Api.IntegrationTests;

/// <summary>
/// A <see cref="FactAttribute"/> that runs only when integration testing is explicitly enabled
/// (<c>TB_INTEGRATION=1</c>) and credentials are resolvable. Otherwise the test is skipped, so the
/// integration suite never makes network calls during ordinary <c>dotnet test</c> runs or in CI
/// for forked pull requests.
/// </summary>
public sealed class IntegrationFactAttribute : FactAttribute
{
    public IntegrationFactAttribute()
    {
        if (Environment.GetEnvironmentVariable("TB_INTEGRATION") is not "1")
        {
            Skip = "Set TB_INTEGRATION=1 and TestingBot credentials to run integration tests.";
        }
        else if (!TestingBotCredentials.TryResolve(null, null, out _, out _))
        {
            Skip = "TestingBot credentials are not configured (TESTINGBOT_KEY/TESTINGBOT_SECRET).";
        }
    }
}
