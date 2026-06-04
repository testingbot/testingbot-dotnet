using System.Collections.Generic;

namespace TestingBot.Api;

/// <summary>Fields to update on a test via <c>PUT /v1/tests/:id</c>. Unset fields are left unchanged.</summary>
public sealed record TestUpdate
{
    /// <summary>A human-readable test name.</summary>
    public string? Name { get; init; }

    /// <summary>Whether the test passed.</summary>
    public bool? Success { get; init; }

    /// <summary>A failure reason or arbitrary status string.</summary>
    public string? StatusMessage { get; init; }

    /// <summary>Arbitrary metadata.</summary>
    public string? Extra { get; init; }

    /// <summary>A build identifier to associate the test with.</summary>
    public string? Build { get; init; }

    /// <summary>When <see langword="true"/>, makes the test publicly viewable via its share URL.</summary>
    public bool? Public { get; init; }

    /// <summary>Tag/group names to attach to the test.</summary>
    public IReadOnlyList<string>? Groups { get; init; }
}

/// <summary>Fields for creating a manual test record via <c>POST /v1/tests</c>.</summary>
public sealed record ManualTestCreate
{
    /// <summary>The test name (required).</summary>
    public required string Name { get; init; }

    /// <summary>Whether the test passed.</summary>
    public bool? Success { get; init; }

    /// <summary>A failure reason or status string.</summary>
    public string? StatusMessage { get; init; }

    /// <summary>Arbitrary metadata.</summary>
    public string? Extra { get; init; }

    /// <summary>A build identifier.</summary>
    public string? Build { get; init; }
}

/// <summary>Fields for creating a Codeless test via <c>POST /v1/lab</c>.</summary>
public sealed record CodelessTestCreate
{
    /// <summary>The test name.</summary>
    public string? Name { get; init; }

    /// <summary>The target URL to test.</summary>
    public string? Url { get; init; }

    /// <summary>A cron expression for scheduled runs.</summary>
    public string? Cron { get; init; }

    /// <summary>Take screenshots at every step.</summary>
    public bool? Screenshot { get; init; }

    /// <summary>Record a video of the test.</summary>
    public bool? Video { get; init; }

    /// <summary>Seconds of idle time before the runner aborts the test.</summary>
    public int? IdleTimeout { get; init; }

    /// <summary>The browser viewport (e.g. <c>1920x1080</c>).</summary>
    public string? ScreenResolution { get; init; }

    /// <summary>A plain-English instruction for the AI test agent.</summary>
    public string? AiPrompt { get; init; }
}

/// <summary>Fields to update on a Codeless test via <c>PUT /v1/lab/:id</c>.</summary>
public sealed record CodelessTestUpdate
{
    /// <summary>A new test name.</summary>
    public string? Name { get; init; }

    /// <summary>A new target URL.</summary>
    public string? Url { get; init; }

    /// <summary>A new cron expression.</summary>
    public string? Cron { get; init; }

    /// <summary>Enable or pause scheduled runs.</summary>
    public bool? Enabled { get; init; }
}

/// <summary>The schedule preset for a Codeless test.</summary>
public enum CodelessScheduleType
{
    /// <summary>Run once at a specific date and time.</summary>
    Once,

    /// <summary>Run daily at a specific time.</summary>
    Daily,

    /// <summary>Run weekly on a specific day and time.</summary>
    Weekly,

    /// <summary>Use a raw cron expression.</summary>
    Custom,
}

/// <summary>A Codeless test schedule via <c>POST /v1/lab/:id/schedule</c>.</summary>
public sealed record CodelessSchedule
{
    /// <summary>The schedule preset.</summary>
    public CodelessScheduleType Type { get; init; }

    /// <summary>The date (<c>YYYY-MM-DD</c>) for <see cref="CodelessScheduleType.Once"/> or weekday for <see cref="CodelessScheduleType.Weekly"/>.</summary>
    public string? Day { get; init; }

    /// <summary>The time (<c>HH:MM</c>).</summary>
    public string? Hour { get; init; }

    /// <summary>A raw 5-field cron expression, used when <see cref="Type"/> is <see cref="CodelessScheduleType.Custom"/>.</summary>
    public string? CronFormat { get; init; }
}

/// <summary>An alert channel for a Codeless test.</summary>
public enum AlertKind
{
    /// <summary>Email.</summary>
    Email,

    /// <summary>HTTP callback.</summary>
    Api,

    /// <summary>SMS.</summary>
    Sms,
}

/// <summary>How frequently a Codeless alert fires.</summary>
public enum AlertLevel
{
    /// <summary>On every failure.</summary>
    Immediately,

    /// <summary>Once per day, as a digest.</summary>
    Daily,
}

/// <summary>A Codeless test alert via <c>POST/PUT /v1/lab/:id/alert</c>.</summary>
public sealed record CodelessAlertInput
{
    /// <summary>The alert channel.</summary>
    public AlertKind Kind { get; init; }

    /// <summary>When to send.</summary>
    public AlertLevel Level { get; init; }

    /// <summary>The destination — email address, callback URL, or phone number.</summary>
    public required string Content { get; init; }
}

/// <summary>A Codeless test daily report config via <c>POST/PUT /v1/lab/:id/report</c>.</summary>
public sealed record CodelessReportInput
{
    /// <summary>The email address that receives the report.</summary>
    public required string Email { get; init; }

    /// <summary>A cron expression for when to send the report.</summary>
    public string? Cron { get; init; }
}

/// <summary>A single step when replacing a Codeless test's recorded steps.</summary>
public sealed record CodelessStepInput
{
    /// <summary>The step order.</summary>
    public int Order { get; init; }

    /// <summary>The Selenium-IDE command.</summary>
    public required string Command { get; init; }

    /// <summary>The Selenium target/locator.</summary>
    public string? Locator { get; init; }

    /// <summary>The Selenium value parameter.</summary>
    public string? Value { get; init; }
}

/// <summary>Fields for creating a Codeless suite via <c>POST /v1/labsuites</c>.</summary>
public sealed record CodelessSuiteCreate
{
    /// <summary>The suite name (required).</summary>
    public required string Name { get; init; }

    /// <summary>A cron expression for scheduled suite runs.</summary>
    public string? Cron { get; init; }

    /// <summary>Take screenshots at every step in every test.</summary>
    public bool? Screenshot { get; init; }

    /// <summary>Record video for tests in the suite.</summary>
    public bool? Video { get; init; }

    /// <summary>Idle timeout in seconds before tests are aborted.</summary>
    public int? IdleTimeout { get; init; }

    /// <summary>The browser viewport for every test in the suite.</summary>
    public string? ScreenResolution { get; init; }
}

/// <summary>Fields for creating a team member via <c>POST /v1/team-management/users</c>.</summary>
public sealed record TeamMemberCreate
{
    /// <summary>The new member's email (required, must be unique).</summary>
    public required string Email { get; init; }

    /// <summary>The initial password (required).</summary>
    public required string Password { get; init; }

    /// <summary>The given name.</summary>
    public string? FirstName { get; init; }

    /// <summary>The family name.</summary>
    public string? LastName { get; init; }

    /// <summary>Max parallel VM sessions for the member.</summary>
    public int? Concurrency { get; init; }

    /// <summary>Max parallel physical-device sessions for the member.</summary>
    public int? ConcurrencyPhysical { get; init; }
}

/// <summary>Fields to update on a team member via <c>PUT /v1/team-management/users/:id</c>.</summary>
public sealed record TeamMemberUpdate
{
    /// <summary>A new given name.</summary>
    public string? FirstName { get; init; }

    /// <summary>A new family name.</summary>
    public string? LastName { get; init; }

    /// <summary>A new email address.</summary>
    public string? Email { get; init; }

    /// <summary>A new password.</summary>
    public string? Password { get; init; }

    /// <summary>Allocated VM credit seconds.</summary>
    public long? Credits { get; init; }

    /// <summary>Allocated physical-device credit seconds.</summary>
    public long? DeviceCredits { get; init; }

    /// <summary>Max parallel VM sessions.</summary>
    public int? Concurrency { get; init; }

    /// <summary>Max parallel physical-device sessions.</summary>
    public int? ConcurrencyPhysical { get; init; }
}

/// <summary>A cross-browser screenshot request via <c>POST /v1/screenshots</c>.</summary>
public sealed record ScreenshotRequest
{
    /// <summary>The page URL to capture (required).</summary>
    public required string Url { get; init; }

    /// <summary>The browser viewport (e.g. <c>1920x1080</c>, required).</summary>
    public required string Resolution { get; init; }

    /// <summary>The browser ids (from <c>GET /v1/browsers</c>) to render with (required, at least one).</summary>
    public required IReadOnlyList<int> BrowserIds { get; init; }

    /// <summary>Seconds to wait after page load before snapping.</summary>
    public int? WaitTime { get; init; }

    /// <summary>Capture the entire scrollable page instead of just the viewport.</summary>
    public bool? FullPage { get; init; }

    /// <summary>A callback URL invoked when the batch finishes processing.</summary>
    public string? CallbackUrl { get; init; }
}

/// <summary>Fields to update on the authenticated account via <c>PUT /v1/user</c>.</summary>
public sealed record UserUpdate
{
    /// <summary>A new given name.</summary>
    public string? FirstName { get; init; }

    /// <summary>A new family name.</summary>
    public string? LastName { get; init; }
}
