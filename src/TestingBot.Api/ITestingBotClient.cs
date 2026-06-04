namespace TestingBot.Api;

/// <summary>
/// The TestingBot REST API client. Exposes one strongly typed sub-client per resource area.
/// A single instance is thread-safe and intended to be reused for the lifetime of your application.
/// </summary>
public interface ITestingBotClient
{
    /// <summary>Unauthenticated configuration data (e.g. firewall IP ranges).</summary>
    IConfigurationClient Configuration { get; }

    /// <summary>The browsers available on the grid.</summary>
    IBrowsersClient Browsers { get; }

    /// <summary>The physical mobile devices in the grid.</summary>
    IDevicesClient Devices { get; }

    /// <summary>The authenticated account.</summary>
    IUserClient User { get; }

    /// <summary>Asynchronous job status (for Codeless trigger endpoints).</summary>
    IJobsClient Jobs { get; }

    /// <summary>Test sessions.</summary>
    ITestsClient Tests { get; }

    /// <summary>Builds (aggregations of tests).</summary>
    IBuildsClient Builds { get; }

    /// <summary>App and binary storage.</summary>
    IStorageClient Storage { get; }

    /// <summary>Cross-browser screenshots.</summary>
    IScreenshotsClient Screenshots { get; }

    /// <summary>TestingBot Tunnels.</summary>
    ITunnelsClient Tunnels { get; }

    /// <summary>Codeless (no-code) tests.</summary>
    ICodelessTestsClient CodelessTests { get; }

    /// <summary>Codeless suites.</summary>
    ICodelessSuitesClient CodelessSuites { get; }

    /// <summary>Team and sub-account management.</summary>
    ITeamClient Team { get; }

    /// <summary>
    /// Computes the MD5 sharing hash (<c>MD5(key:secret:identifier)</c>) used to build public,
    /// read-only share URLs for a session or build. This is a local computation; no request is made.
    /// </summary>
    /// <param name="identifier">The session id or build identifier to share.</param>
    /// <returns>The lowercase hexadecimal MD5 hash.</returns>
    string GetSharingAuthHash(string identifier);
}
