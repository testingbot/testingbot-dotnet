# Changelog

All notable changes to this project are documented here. The format is based on
[Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project adheres to
[Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Initial release of the official TestingBot .NET SDK.
- `TestingBot.Api` core package (targets `net8.0` and `net9.0`, zero third-party dependencies):
  - `ITestingBotClient` / `TestingBotClient` with strongly typed sub-clients for Tests, Builds,
    Storage, Screenshots, Tunnels, Devices, Browsers, Codeless tests, Codeless suites, Jobs, Team,
    User, and Configuration.
  - Credential resolution from constructor arguments, `~/.testingbot`, or `TESTINGBOT_*` / `TB_*`
    environment variables.
  - Async/await throughout with `CancellationToken` support and `IAsyncEnumerable<T>`
    auto-pagination helpers.
  - Automatic retries with exponential backoff and jitter, honoring `Retry-After`.
  - Streamed storage uploads with progress reporting.
  - Typed exception hierarchy mapping HTTP status codes.
  - `System.Text.Json` source-generated, trimming/AOT-friendly serialization.
- `TestingBot.Api.DependencyInjection` package: `AddTestingBot()` for `IServiceCollection` with
  `IHttpClientFactory` integration and `IConfiguration` binding.
