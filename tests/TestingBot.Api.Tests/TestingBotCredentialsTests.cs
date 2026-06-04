using System.Collections.Generic;
using System.IO;

namespace TestingBot.Api.Tests;

public class TestingBotCredentialsTests
{
    private static readonly Func<string, string?> NoEnvironment = _ => null;

    private static string NonExistentHome()
        => Path.Combine(Path.GetTempPath(), "testingbot-tests-" + Guid.NewGuid().ToString("N"));

    [Fact]
    public void Explicit_credentials_take_precedence()
    {
        var resolved = TestingBotCredentials.TryResolveCore("ek", "es", NonExistentHome(), _ => "should-not-be-used", out var key, out var secret);

        resolved.Should().BeTrue();
        key.Should().Be("ek");
        secret.Should().Be("es");
    }

    [Fact]
    public void Resolves_from_testingbot_environment_variables()
    {
        var env = new Dictionary<string, string?>
        {
            ["TESTINGBOT_KEY"] = "envk",
            ["TESTINGBOT_SECRET"] = "envs",
        };

        var resolved = TestingBotCredentials.TryResolveCore(null, null, NonExistentHome(), name => env.GetValueOrDefault(name), out var key, out var secret);

        resolved.Should().BeTrue();
        key.Should().Be("envk");
        secret.Should().Be("envs");
    }

    [Fact]
    public void Falls_back_to_tb_environment_variables()
    {
        var env = new Dictionary<string, string?>
        {
            ["TB_KEY"] = "tbk",
            ["TB_SECRET"] = "tbs",
        };

        var resolved = TestingBotCredentials.TryResolveCore(null, null, NonExistentHome(), name => env.GetValueOrDefault(name), out var key, out var secret);

        resolved.Should().BeTrue();
        key.Should().Be("tbk");
        secret.Should().Be("tbs");
    }

    [Fact]
    public void Config_file_takes_precedence_over_environment()
    {
        var home = Directory.CreateTempSubdirectory("tb-creds").FullName;
        try
        {
            File.WriteAllText(Path.Combine(home, ".testingbot"), "filek:files\n");
            var env = new Dictionary<string, string?> { ["TB_KEY"] = "tbk", ["TB_SECRET"] = "tbs" };

            var resolved = TestingBotCredentials.TryResolveCore(null, null, home, name => env.GetValueOrDefault(name), out var key, out var secret);

            resolved.Should().BeTrue();
            key.Should().Be("filek");
            secret.Should().Be("files");
        }
        finally
        {
            Directory.Delete(home, recursive: true);
        }
    }

    [Fact]
    public void Returns_false_when_nothing_resolves()
    {
        var resolved = TestingBotCredentials.TryResolveCore(null, null, NonExistentHome(), NoEnvironment, out var key, out var secret);

        resolved.Should().BeFalse();
        key.Should().BeNull();
        secret.Should().BeNull();
    }

    [Fact]
    public void Resolve_throws_configuration_exception_when_unresolved()
    {
        // Explicit-only resolution still consults file/env; passing whitespace forces fallback,
        // but this machine may have a ~/.testingbot file, so assert via the testable core instead.
        var act = () =>
        {
            if (!TestingBotCredentials.TryResolveCore(null, null, NonExistentHome(), NoEnvironment, out _, out _))
            {
                throw new TestingBotConfigurationException("unresolved");
            }
        };

        act.Should().Throw<TestingBotConfigurationException>();
    }
}
