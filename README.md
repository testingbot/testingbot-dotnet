# TestingBot .NET SDK

[![NuGet](https://img.shields.io/nuget/v/TestingBot.Api.svg)](https://www.nuget.org/packages/TestingBot.Api/)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)

The official .NET SDK for the [TestingBot](https://testingbot.com) REST API. Manage tests, builds,
app storage, tunnels, devices, screenshots, Codeless tests/suites, jobs, and team accounts from C#.

- **Async** all the way down, with `CancellationToken` support everywhere
- **Thread-safe** — construct one client and reuse it across your application
- **Strongly typed** models — no `dynamic`, no untyped dictionaries
- **Zero third-party dependencies** in the core package
- **Resilient** — automatic retries with exponential backoff and `Retry-After` support
- Optional **dependency-injection** package wiring up `IHttpClientFactory`
- Targets **.NET 8** and **.NET 9**

## Packages

| Package | Purpose |
|---|---|
| [`TestingBot.Api`](https://www.nuget.org/packages/TestingBot.Api/) | Core client and models (zero third-party dependencies) |
| [`TestingBot.Api.DependencyInjection`](https://www.nuget.org/packages/TestingBot.Api.DependencyInjection/) | `AddTestingBot()` for `IServiceCollection` / `IHttpClientFactory` |

## Installation

```bash
dotnet add package TestingBot.Api
# optional, for ASP.NET Core / Worker apps:
dotnet add package TestingBot.Api.DependencyInjection
```

## Authentication

Provide your [API key and secret](https://testingbot.com/members/user/edit) directly, or let the SDK
resolve them (in order) from a `~/.testingbot` file (`key:secret`), the `TESTINGBOT_KEY`/
`TESTINGBOT_SECRET` environment variables, or `TB_KEY`/`TB_SECRET`.

```csharp
using TestingBot.Api;

var client = new TestingBotClient("YOUR_KEY", "YOUR_SECRET");
// or resolve from ~/.testingbot or environment variables:
var client = TestingBotClient.FromEnvironment();
```

## Quick start

```csharp
using TestingBot.Api;

using var client = new TestingBotClient("YOUR_KEY", "YOUR_SECRET");

// Who am I?
var user = await client.User.GetAsync();
Console.WriteLine($"{user.Email} — {user.Seconds} seconds remaining");

// A single test, by id or WebDriver session id
var test = await client.Tests.GetAsync("a1b2c3d4-...");
Console.WriteLine($"{test.Name}: {test.Success}");

// Mark a test passed and tag it
await client.Tests.UpdateAsync(test.SessionId!, new TestUpdate
{
    Success = true,
    StatusMessage = "All assertions passed",
    Build = "ci-1421",
    Groups = ["smoke", "checkout"],
});
```

## Pagination

List endpoints return a `TestingBotPage<T>` (the items plus `meta`), or you can stream every item
across all pages with `ListAllAsync`:

```csharp
// One page at a time
var page = await client.Tests.ListAsync(new TestListOptions { Count = 50, Build = "ci-1421" });
Console.WriteLine($"{page.Count} of {page.Meta.Total}");

// Or iterate everything (pages fetched lazily)
await foreach (var t in client.Tests.ListAllAsync(new TestListOptions { Build = "ci-1421" }))
{
    Console.WriteLine($"{t.Id}: {t.Success}");
}
```

## Uploading apps (storage)

Uploads are streamed (never fully buffered in memory), support progress reporting, and use a longer
timeout than ordinary calls.

```csharp
var progress = new Progress<long>(bytes => Console.WriteLine($"{bytes:N0} bytes sent"));
var app = await client.Storage.UploadAsync(new FileInfo("app.apk"), progress);
Console.WriteLine(app.AppUrl); // tb://<appkey> — pass as the `app` capability

// Replace the binary behind an app key without changing the tb:// URL:
await client.Storage.ReplaceAsync(app.AppKey!, new FileInfo("app-new.apk"));
```

## Codeless tests and jobs

```csharp
var run = await client.CodelessSuites.TriggerAsync(suiteId);
Job job = await client.Jobs.WaitForCompletionAsync(run.JobId, timeout: TimeSpan.FromMinutes(10));
Console.WriteLine(job.Success);
```

## Error handling

Every failure derives from `TestingBotException`; catch a specific type to react to a failure mode:

```csharp
try
{
    await client.Tunnels.StopAsync(tunnelId);
}
catch (TestingBotRateLimitException ex)
{
    await Task.Delay(ex.RetryAfter ?? TimeSpan.FromSeconds(30));
}
catch (TestingBotAuthenticationException)
{
    Console.Error.WriteLine("Check your API key and secret.");
}
catch (TestingBotValidationException ex)
{
    foreach (var error in ex.ValidationErrors)
    {
        Console.Error.WriteLine(error);
    }
}
```

| Exception | HTTP status |
|---|---|
| `TestingBotAuthenticationException` | 401 |
| `TestingBotPaymentRequiredException` | 402 (insufficient credits) |
| `TestingBotForbiddenException` | 403 (read-only account / not admin) |
| `TestingBotNotFoundException` | 404 |
| `TestingBotValidationException` | 400 |
| `TestingBotRateLimitException` | 429 (carries `RetryAfter`) |
| `TestingBotApiException` | other 4xx/5xx, transport failures |
| `TestingBotConfigurationException` | missing credentials / invalid options (no HTTP) |

## Dependency injection

```csharp
using Microsoft.Extensions.DependencyInjection;

builder.Services.AddTestingBot(options =>
{
    options.ApiKey = builder.Configuration["TestingBot:ApiKey"];
    options.ApiSecret = builder.Configuration["TestingBot:ApiSecret"];
});

// or bind from configuration:
builder.Services.AddTestingBot(builder.Configuration.GetSection("TestingBot"));
```

Then inject `ITestingBotClient` anywhere. The client is registered as a singleton backed by
`IHttpClientFactory` with the SDK's authentication and retry handlers.

## Configuration

| Option | Default | Description |
|---|---|---|
| `BaseAddress` | `https://api.testingbot.com/v1/` | Override for sandbox/private deployments |
| `Timeout` | 100s | Per-request timeout for ordinary calls |
| `UploadTimeout` | 30m | Timeout for storage uploads |
| `MaxRetries` | 3 | Retries for transient failures (429/5xx) on idempotent requests |
| `RetryBaseDelay` | 1s | Base delay for exponential backoff |
| `RespectRetryAfter` | `true` | Honor the server's `Retry-After` header |
| `DefaultPageSize` | 50 | Page size used by `ListAllAsync` helpers |

## API coverage

`Tests`, `Builds`, `Storage`, `Screenshots`, `Tunnels`, `Devices`, `Browsers`, `CodelessTests`,
`CodelessSuites`, `Jobs`, `Team`, `User`, `Configuration`, plus `GetSharingAuthHash` for building
public share URLs.

## Contributing & development

```bash
dotnet build
dotnet test
dotnet format --verify-no-changes
```

Integration tests run only when `TESTINGBOT_KEY`/`TESTINGBOT_SECRET` are set and are otherwise skipped.

## License

MIT — see [LICENSE](LICENSE).
