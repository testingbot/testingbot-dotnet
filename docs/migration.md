# Migration & forward-compatibility notes

## API versioning

The API version lives in `TestingBotClientOptions.BaseAddress` (default
`https://api.testingbot.com/v1/`). A future `/v2` is a configuration change or a parallel client —
not a rewrite. Point a client at a sandbox or private deployment by overriding `BaseAddress`.

## Forward compatibility

The SDK is built to tolerate API drift without breaking older clients:

- **Unknown JSON fields are ignored.** New response fields won't break deserialization.
- **Tolerant converters.** Booleans accept `true`/`false`/`0`/`1`/`"true"`; timestamps accept
  ISO-8601 or Unix epoch; polymorphic fields (`video`, `thumbs`, `logs`, `groups`) are handled.
- **String-typed lifecycle states.** Fields like test `state` and job `status` are exposed as
  strings so new server values never throw.
- **Options objects** for list/filter parameters (e.g. `TestListOptions`) mean new query parameters
  can be added without changing method signatures.

## Recorded fixtures

Model shapes are derived from real API responses. The serialization test suite deserializes
representative payloads (including the API's known quirks); when the server changes a shape, those
tests surface it as a failure rather than a silent data-loss bug.

## Breaking-change policy

The library follows Semantic Versioning. Public API compatibility is enforced at build time via
package validation, and the public surface is reviewed on every change. Members are deprecated with
`[Obsolete]` for at least one minor release before removal.
