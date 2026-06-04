# Future API surface

The v1 SDK intentionally covers the **documented, public** TestingBot REST API. The endpoints below
exist in the API but are deliberately **not** exposed yet, because they are internal/runner-facing,
undocumented, or live on a different service. They are tracked here as candidates for future,
additive releases (likely in separate packages such as `TestingBot.Api.AppAutomate`).

## App-automate framework runners (currently internal)

Upload/run/report endpoints for the native test frameworks, used today mainly by the TestingBot CLI:

- **Espresso** — `/v1/app-automate/espresso/*`
- **XCUITest** — `/v1/app-automate/xcuitest/*`
- **Maestro** (app and web) — `/v1/app-automate/maestro/*`, `/v1/web-automate/maestro/*`
- **Cypress** — `/v1/cypress/*`

These are marked `hidden` in the canonical API and their request/response shapes are not yet stable
for public consumption.

## Manual sessions

- `/v1/manual_session` (update, ping) — used by the dashboard's in-browser session viewer.

## Cloud session creation (CDP / Playwright)

- `POST https://cloud.testingbot.com/session` — creates a remote browser session and returns a
  `cdp_url`. This lives on a **different service** (`cloud.testingbot.com`, not `api.testingbot.com/v1`)
  and would be modeled as a separate client/package if added.

## Internal callbacks (will not be exposed)

Tunnel lifecycle callbacks (`/v1/tunnel/{test,intern-check,gone,ready}`) and various runner
step/stop callbacks are infrastructure hooks and are out of scope for any SDK release.
