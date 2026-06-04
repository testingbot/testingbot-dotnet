using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace TestingBot.Api.Serialization;

/// <summary>
/// Source-generated <see cref="JsonSerializerContext"/> for every type the SDK serializes.
/// Using source generation keeps the SDK trimming- and AOT-friendly and avoids reflection at runtime.
/// Snake-case is the default naming policy; individual model properties override it with
/// <see cref="JsonPropertyNameAttribute"/> where the API uses a different casing.
/// </summary>
[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    NumberHandling = JsonNumberHandling.AllowReadingFromString,
    Converters = new[] { typeof(FlexibleBooleanConverter), typeof(TolerantDateTimeOffsetConverter) })]
// Infrastructure types.
[JsonSerializable(typeof(ApiErrorPayload))]
[JsonSerializable(typeof(AckPayload))]
[JsonSerializable(typeof(PageMeta))]
[JsonSerializable(typeof(List<string>))]
[JsonSerializable(typeof(List<long>))]
[JsonSerializable(typeof(string))]
[JsonSerializable(typeof(bool))]
[JsonSerializable(typeof(JsonElement))]
// Models (singletons and their List<> forms for paginated/bare-array responses).
[JsonSerializable(typeof(Models.Browser))]
[JsonSerializable(typeof(List<Models.Browser>))]
[JsonSerializable(typeof(Models.Device))]
[JsonSerializable(typeof(List<Models.Device>))]
[JsonSerializable(typeof(Models.Build))]
[JsonSerializable(typeof(List<Models.Build>))]
[JsonSerializable(typeof(Models.TestCase))]
[JsonSerializable(typeof(List<Models.TestCase>))]
[JsonSerializable(typeof(Models.TestThumb))]
[JsonSerializable(typeof(List<Models.TestThumb>))]
[JsonSerializable(typeof(Models.Job))]
[JsonSerializable(typeof(Models.User))]
[JsonSerializable(typeof(Models.UserKeys))]
[JsonSerializable(typeof(Models.Tunnel))]
[JsonSerializable(typeof(List<Models.Tunnel>))]
[JsonSerializable(typeof(Models.CodelessTest))]
[JsonSerializable(typeof(List<Models.CodelessTest>))]
[JsonSerializable(typeof(Models.CodelessAlert))]
[JsonSerializable(typeof(Models.CodelessStep))]
[JsonSerializable(typeof(List<Models.CodelessStep>))]
[JsonSerializable(typeof(Models.CodelessSuite))]
[JsonSerializable(typeof(List<Models.CodelessSuite>))]
[JsonSerializable(typeof(Models.TeamConcurrency))]
[JsonSerializable(typeof(Models.TeamConcurrencyResponse))]
[JsonSerializable(typeof(Models.ConcurrencySlots))]
[JsonSerializable(typeof(Models.TeamMember))]
[JsonSerializable(typeof(List<Models.TeamMember>))]
[JsonSerializable(typeof(Models.TeamCredentialReset))]
[JsonSerializable(typeof(Models.Screenshot))]
[JsonSerializable(typeof(List<Models.Screenshot>))]
[JsonSerializable(typeof(Models.ScreenshotImage))]
[JsonSerializable(typeof(Models.StorageFile))]
[JsonSerializable(typeof(List<Models.StorageFile>))]
internal sealed partial class TestingBotJsonContext : JsonSerializerContext;

/// <summary>Central access to the SDK's configured JSON options and per-type metadata.</summary>
internal static class TestingBotJson
{
    /// <summary>The shared, source-generated serializer options.</summary>
    public static JsonSerializerOptions Options => TestingBotJsonContext.Default.Options;

    /// <summary>Returns strongly typed metadata for <typeparamref name="T"/> from the source-generated context.</summary>
    /// <typeparam name="T">A type registered in <see cref="TestingBotJsonContext"/>.</typeparam>
    public static JsonTypeInfo<T> TypeInfo<T>() => (JsonTypeInfo<T>)Options.GetTypeInfo(typeof(T));
}
