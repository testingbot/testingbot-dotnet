// TestingBot .NET SDK — runnable samples.
//
// Set TESTINGBOT_KEY and TESTINGBOT_SECRET (or TB_KEY/TB_SECRET), then run:
//   dotnet run --project samples/TestingBot.Api.Samples
using TestingBot.Api;

if (!TestingBotCredentials.TryResolve(null, null, out _, out _))
{
    Console.WriteLine("Set TESTINGBOT_KEY and TESTINGBOT_SECRET to run these samples.");
    return;
}

using var client = TestingBotClient.FromEnvironment();

// 1) Account info
var user = await client.User.GetAsync();
Console.WriteLine($"Signed in as {user.Email} on the '{user.Plan}' plan ({user.Seconds} seconds left).");

// 2) Available browsers and devices
var browsers = await client.Browsers.ListAsync();
Console.WriteLine($"{browsers.Count} browser environments available.");

var devices = await client.Devices.ListAvailableAsync();
Console.WriteLine($"{devices.Count} mobile devices available right now.");

// 3) Recent tests (streamed across pages, capped here for brevity)
Console.WriteLine("Recent tests:");
var shown = 0;
await foreach (var test in client.Tests.ListAllAsync(new TestListOptions { Count = 20 }))
{
    Console.WriteLine($"  [{test.Id}] {test.Name} — success={test.Success}, state={test.State}");
    if (++shown >= 5)
    {
        break;
    }
}

// 4) Builds
var builds = await client.Builds.ListAsync(new PageOptions { Count = 5 });
Console.WriteLine($"{builds.Meta.Total} builds total; newest page has {builds.Count}.");

// 5) Firewall IP ranges (unauthenticated)
var ranges = await client.Configuration.GetIpRangesAsync();
Console.WriteLine($"TestingBot uses {ranges.Count} IP ranges.");

// 6) A sharing hash for embedding a public, read-only view of a session
var firstPage = await client.Tests.ListAsync(new TestListOptions { Count = 1 });
if (firstPage.Count > 0 && firstPage.Data[0].SessionId is { } sessionId)
{
    Console.WriteLine($"Share hash for {sessionId}: {client.GetSharingAuthHash(sessionId)}");
}

Console.WriteLine("Done.");
