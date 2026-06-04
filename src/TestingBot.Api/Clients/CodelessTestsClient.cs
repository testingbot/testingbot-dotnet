using System.Collections.Generic;
using System.Net.Http;
using TestingBot.Api.Http;
using TestingBot.Api.Models;
using TestingBot.Api.Serialization;

namespace TestingBot.Api;

/// <summary>Operations for Codeless (recorded, no-code) tests via the <c>/v1/lab</c> endpoints.</summary>
public interface ICodelessTestsClient
{
    /// <summary>Lists Codeless tests, newest first (<c>GET /v1/lab</c>).</summary>
    Task<TestingBotPage<CodelessTest>> ListAsync(PageOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Lazily iterates every Codeless test across all pages.</summary>
    IAsyncEnumerable<CodelessTest> ListAllAsync(int? pageSize = null, CancellationToken cancellationToken = default);

    /// <summary>Gets a Codeless test by id (<c>GET /v1/lab/:id</c>).</summary>
    Task<CodelessTest> GetAsync(long testId, CancellationToken cancellationToken = default);

    /// <summary>Creates a Codeless test (<c>POST /v1/lab</c>). Returns the new test id.</summary>
    Task<long> CreateAsync(CodelessTestCreate request, CancellationToken cancellationToken = default);

    /// <summary>Updates a Codeless test's metadata (<c>PUT /v1/lab/:id</c>).</summary>
    Task<bool> UpdateAsync(long testId, CodelessTestUpdate request, CancellationToken cancellationToken = default);

    /// <summary>Deletes a Codeless test (<c>DELETE /v1/lab/:id</c>).</summary>
    Task<bool> DeleteAsync(long testId, CancellationToken cancellationToken = default);

    /// <summary>Runs a Codeless test now (<c>POST /v1/lab/:id/trigger</c>).</summary>
    Task<TriggerResult> TriggerAsync(long testId, string? url = null, CancellationToken cancellationToken = default);

    /// <summary>Runs every Codeless test on the account (<c>POST /v1/lab/trigger_all</c>).</summary>
    Task<TriggerResult> TriggerAllAsync(string? url = null, CancellationToken cancellationToken = default);

    /// <summary>Stops a running Codeless test (<c>PUT /v1/lab/:id/stop</c>).</summary>
    Task<bool> StopAsync(long testId, int? browserId = null, CancellationToken cancellationToken = default);

    /// <summary>Sets or updates a Codeless test's schedule (<c>POST /v1/lab/:id/schedule</c>).</summary>
    Task<bool> ScheduleAsync(long testId, CodelessSchedule schedule, CancellationToken cancellationToken = default);

    /// <summary>Lists a Codeless test's recorded steps (<c>GET /v1/lab/:id/steps</c>).</summary>
    Task<TestingBotPage<CodelessStep>> GetStepsAsync(long testId, PageOptions? options = null, CancellationToken cancellationToken = default);

    /// <summary>Replaces a Codeless test's steps (<c>POST /v1/lab/:id/steps</c>).</summary>
    Task<bool> SetStepsAsync(long testId, IReadOnlyList<CodelessStepInput> steps, CancellationToken cancellationToken = default);

    /// <summary>Gets the browsers a Codeless test runs on (<c>GET /v1/lab/:id/browsers</c>).</summary>
    Task<IReadOnlyList<Browser>> GetBrowsersAsync(long testId, CancellationToken cancellationToken = default);

    /// <summary>Replaces the browsers a Codeless test runs on (<c>POST /v1/lab/:id/browsers</c>).</summary>
    Task<bool> SetBrowsersAsync(long testId, IReadOnlyList<int> browserIds, CancellationToken cancellationToken = default);

    /// <summary>Adds an alert to a Codeless test (<c>POST /v1/lab/:id/alert</c>).</summary>
    Task<bool> AddAlertAsync(long testId, CodelessAlertInput alert, CancellationToken cancellationToken = default);

    /// <summary>Updates a Codeless test's alert (<c>PUT /v1/lab/:id/alert</c>).</summary>
    Task<bool> UpdateAlertAsync(long testId, CodelessAlertInput alert, CancellationToken cancellationToken = default);

    /// <summary>Adds a daily report config to a Codeless test (<c>POST /v1/lab/:id/report</c>).</summary>
    Task<bool> AddReportAsync(long testId, CodelessReportInput report, CancellationToken cancellationToken = default);

    /// <summary>Updates a Codeless test's report config (<c>PUT /v1/lab/:id/report</c>).</summary>
    Task<bool> UpdateReportAsync(long testId, CodelessReportInput report, CancellationToken cancellationToken = default);
}

internal sealed class CodelessTestsClient(TestingBotConnection connection) : ResourceClient(connection), ICodelessTestsClient
{
    public Task<TestingBotPage<CodelessTest>> ListAsync(PageOptions? options = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryString().Add("offset", options?.Offset).Add("count", options?.Count);
        return Connection.GetPageAsync<CodelessTest>("lab", query, cancellationToken);
    }

    public IAsyncEnumerable<CodelessTest> ListAllAsync(int? pageSize = null, CancellationToken cancellationToken = default)
        => PaginateAsync((offset, size, token) => ListAsync(new PageOptions { Offset = offset, Count = size }, token), pageSize, cancellationToken);

    public Task<CodelessTest> GetAsync(long testId, CancellationToken cancellationToken = default)
        => Connection.GetAsync<CodelessTest>($"lab/{testId}", query: null, cancellationToken);

    public async Task<long> CreateAsync(CodelessTestCreate request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var form = new FormContentBuilder()
            .Add("test", "name", request.Name)
            .Add("test", "url", request.Url)
            .Add("test", "cron", request.Cron)
            .Add("test", "screenshot", request.Screenshot)
            .Add("test", "video", request.Video)
            .Add("test", "idletimeout", request.IdleTimeout)
            .Add("test", "screenresolution", request.ScreenResolution)
            .Add("test", "ai_prompt", request.AiPrompt);

        var element = await Connection.SendRawAsync(HttpMethod.Post, "lab", form.Build(), query: null, isUpload: false, cancellationToken).ConfigureAwait(false);
        EnsureSuccess(element);
        return element.GetInt64OrDefault("lab_test_id");
    }

    public Task<bool> UpdateAsync(long testId, CodelessTestUpdate request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var form = new FormContentBuilder()
            .Add("test", "name", request.Name)
            .Add("test", "url", request.Url)
            .Add("test", "cron", request.Cron)
            .Add("test", "enabled", request.Enabled);
        return Connection.SendAckAsync(HttpMethod.Put, $"lab/{testId}", form.Build(), query: null, cancellationToken);
    }

    public Task<bool> DeleteAsync(long testId, CancellationToken cancellationToken = default)
        => Connection.SendAckAsync(HttpMethod.Delete, $"lab/{testId}", content: null, query: null, cancellationToken);

    public async Task<TriggerResult> TriggerAsync(long testId, string? url = null, CancellationToken cancellationToken = default)
    {
        var form = new FormContentBuilder().Add("url", url);
        var element = await Connection.SendRawAsync(HttpMethod.Post, $"lab/{testId}/trigger", form.Build(), query: null, isUpload: false, cancellationToken).ConfigureAwait(false);
        return ToTriggerResult(element);
    }

    public async Task<TriggerResult> TriggerAllAsync(string? url = null, CancellationToken cancellationToken = default)
    {
        var form = new FormContentBuilder().Add("url", url);
        var element = await Connection.SendRawAsync(HttpMethod.Post, "lab/trigger_all", form.Build(), query: null, isUpload: false, cancellationToken).ConfigureAwait(false);
        return ToTriggerResult(element);
    }

    public Task<bool> StopAsync(long testId, int? browserId = null, CancellationToken cancellationToken = default)
    {
        var form = new FormContentBuilder().Add("browser_id", browserId);
        return Connection.SendAckAsync(HttpMethod.Put, $"lab/{testId}/stop", form.Build(), query: null, cancellationToken);
    }

    public Task<bool> ScheduleAsync(long testId, CodelessSchedule schedule, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(schedule);
        var form = new FormContentBuilder()
            .Add("type", Render(schedule.Type))
            .Add("day", schedule.Day)
            .Add("hour", schedule.Hour)
            .Add("cronFormat", schedule.CronFormat);
        return Connection.SendAckAsync(HttpMethod.Post, $"lab/{testId}/schedule", form.Build(), query: null, cancellationToken);
    }

    public Task<TestingBotPage<CodelessStep>> GetStepsAsync(long testId, PageOptions? options = null, CancellationToken cancellationToken = default)
    {
        var query = new QueryString().Add("offset", options?.Offset).Add("count", options?.Count);
        return Connection.GetPageAsync<CodelessStep>($"lab/{testId}/steps", query, cancellationToken);
    }

    public Task<bool> SetStepsAsync(long testId, IReadOnlyList<CodelessStepInput> steps, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(steps);
        var content = JsonContentFactory.Create(writer =>
        {
            writer.WriteStartObject();
            writer.WriteStartArray("steps");
            foreach (var step in steps)
            {
                writer.WriteStartObject();
                writer.WriteNumber("order", step.Order);
                writer.WriteString("cmd", step.Command);
                writer.WriteString("locator", step.Locator);
                writer.WriteString("value", step.Value);
                writer.WriteEndObject();
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        });

        return Connection.SendAckAsync(HttpMethod.Post, $"lab/{testId}/steps", content, query: null, cancellationToken);
    }

    public Task<IReadOnlyList<Browser>> GetBrowsersAsync(long testId, CancellationToken cancellationToken = default)
        => Connection.GetListAsync<Browser>($"lab/{testId}/browsers", query: null, cancellationToken);

    public Task<bool> SetBrowsersAsync(long testId, IReadOnlyList<int> browserIds, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(browserIds);
        var form = new FormContentBuilder().Add("browser_ids", string.Join(',', browserIds));
        return Connection.SendAckAsync(HttpMethod.Post, $"lab/{testId}/browsers", form.Build(), query: null, cancellationToken);
    }

    public Task<bool> AddAlertAsync(long testId, CodelessAlertInput alert, CancellationToken cancellationToken = default)
        => SendAlert(HttpMethod.Post, testId, alert, cancellationToken);

    public Task<bool> UpdateAlertAsync(long testId, CodelessAlertInput alert, CancellationToken cancellationToken = default)
        => SendAlert(HttpMethod.Put, testId, alert, cancellationToken);

    public Task<bool> AddReportAsync(long testId, CodelessReportInput report, CancellationToken cancellationToken = default)
        => SendReport(HttpMethod.Post, testId, report, cancellationToken);

    public Task<bool> UpdateReportAsync(long testId, CodelessReportInput report, CancellationToken cancellationToken = default)
        => SendReport(HttpMethod.Put, testId, report, cancellationToken);

    private Task<bool> SendAlert(HttpMethod method, long testId, CodelessAlertInput alert, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(alert);
        var form = new FormContentBuilder()
            .Add("kind", Render(alert.Kind))
            .Add("level", Render(alert.Level))
            .Add("content", alert.Content);
        return Connection.SendAckAsync(method, $"lab/{testId}/alert", form.Build(), query: null, cancellationToken);
    }

    private Task<bool> SendReport(HttpMethod method, long testId, CodelessReportInput report, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(report);
        var form = new FormContentBuilder().Add("email", report.Email).Add("cron", report.Cron);
        return Connection.SendAckAsync(method, $"lab/{testId}/report", form.Build(), query: null, cancellationToken);
    }

    private static TriggerResult ToTriggerResult(System.Text.Json.JsonElement element)
        => new() { Success = element.GetBoolOrFalse("success"), JobId = element.GetInt64OrDefault("job_id") };

    private static void EnsureSuccess(System.Text.Json.JsonElement element)
    {
        if (element.ValueKind == System.Text.Json.JsonValueKind.Object
            && element.TryGetProperty("success", out var success)
            && success.ValueKind == System.Text.Json.JsonValueKind.False)
        {
            var message = element.GetStringOrNull("errors") ?? "The TestingBot API rejected the request.";
            throw new TestingBotValidationException(message);
        }
    }

    private static string Render(AlertKind kind) => kind switch
    {
        AlertKind.Email => "EMAIL",
        AlertKind.Api => "API",
        AlertKind.Sms => "SMS",
        _ => "EMAIL",
    };

    private static string Render(AlertLevel level) => level switch
    {
        AlertLevel.Immediately => "IMMEDIATELY",
        AlertLevel.Daily => "DAILY",
        _ => "IMMEDIATELY",
    };

    private static string Render(CodelessScheduleType type) => type switch
    {
        CodelessScheduleType.Once => "once",
        CodelessScheduleType.Daily => "daily",
        CodelessScheduleType.Weekly => "weekly",
        CodelessScheduleType.Custom => "custom",
        _ => "once",
    };
}
