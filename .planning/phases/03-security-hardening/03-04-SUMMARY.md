---
phase: 03-security-hardening
plan: "04"
subsystem: infra
tags: [httpclient, yandex, api-key, security, moderation]

requires:
  - phase: 03-security-hardening
    provides: Security hardening plans 03-01 through 03-03

provides:
  - Yandex API key moved from URL query string to X-Yandex-API-Key DefaultRequestHeaders header

affects: [04-performance-optimization]

tech-stack:
  added: []
  patterns:
    - "Typed HttpClient DefaultRequestHeaders for API key injection — key set once at DI registration, never in URLs"

key-files:
  created: []
  modified:
    - CoffeePeek.Moderation.Infrastructure/DependencyInjection.cs
    - CoffeePeek.Moderation.Infrastructure/Services/YandexGeocodingService.cs

key-decisions:
  - "SEC-04: Option A selected — API key moved to X-Yandex-API-Key DefaultRequestHeaders header; URL is now a relative path with no apikey param"
  - "BaseUrl already set as client.BaseAddress — relative URL ?geocode=... resolves correctly without repeating BaseUrl in GeocodeAsync"
  - "_options field retained in YandexGeocodingService (may hold other options); ApiKey usage removed from URL construction"

patterns-established:
  - "API keys for external HTTP services: set in DefaultRequestHeaders at DI registration, never embed in request URLs"

requirements-completed:
  - SEC-04

duration: 8min
completed: "2026-05-18"
---

# Phase 3 Plan 04: Yandex API Key Header Migration Summary

**Yandex geocoding API key removed from outgoing URL query string and placed in X-Yandex-API-Key DefaultRequestHeaders, eliminating key exposure in ASP.NET Core HTTP diagnostics, Serilog, Sentry breadcrumbs, and OTLP traces**

## Performance

- **Duration:** ~8 min
- **Started:** 2026-05-18T19:15:00Z
- **Completed:** 2026-05-18T19:23:00Z
- **Tasks:** 1 (Task 2 — Task 1 was a checkpoint, pre-resolved by user)
- **Files modified:** 2

## Accomplishments

- Removed `?apikey={key}` from the outgoing Yandex geocoding URL — API key no longer appears in any logged URL, trace, or diagnostic output
- Added `client.DefaultRequestHeaders.Add("X-Yandex-API-Key", yandexOptions.ApiKey)` in the `AddHttpClient` lambda in DependencyInjection.cs — key is set once at DI registration time
- URL in `GeocodeAsync` changed from `$"{_options.BaseUrl}?apikey={_options.ApiKey}&geocode=..."` to `$"?geocode={encodedAddress}&format=json&results=1"` — resolves correctly against `client.BaseAddress`

## Task Commits

1. **Task 2: Move Yandex API key from URL to DefaultRequestHeaders** - `ec698c0` (fix)

## Files Created/Modified

- `CoffeePeek.Moderation.Infrastructure/DependencyInjection.cs` — Added `client.DefaultRequestHeaders.Add("X-Yandex-API-Key", yandexOptions.ApiKey)` with explanatory comment to AddHttpClient lambda
- `CoffeePeek.Moderation.Infrastructure/Services/YandexGeocodingService.cs` — Replaced full URL with apikey param with relative URL `?geocode={encodedAddress}&format=json&results=1`

## Decisions Made

- Option A (header approach) selected by user at checkpoint: API key moved to `X-Yandex-API-Key` HTTP header rather than kept in URL with log sanitization (Option B)
- `_options` field retained in `YandexGeocodingService` — field is still in place even though `_options.ApiKey` is no longer used in URL construction; `_options.BaseUrl` is no longer needed either since `BaseAddress` handles it, but the field is left as-is per plan instructions

## Verification

```
grep "apikey" CoffeePeek.Moderation.Infrastructure/Services/YandexGeocodingService.cs
# → 0 matches (key removed from URL)

grep "DefaultRequestHeaders" CoffeePeek.Moderation.Infrastructure/DependencyInjection.cs
# → line 18: client.DefaultRequestHeaders.Add("X-Yandex-API-Key", yandexOptions.ApiKey);

grep "X-Yandex-API-Key" CoffeePeek.Moderation.Infrastructure/DependencyInjection.cs
# → line 18: client.DefaultRequestHeaders.Add("X-Yandex-API-Key", yandexOptions.ApiKey);

dotnet build CoffeePeek.ModerationService/CoffeePeek.ModerationService.csproj --no-restore
# → Предупреждений: 82 / Ошибок: 0

dotnet build CoffeePeek.slnx --no-restore
# → Предупреждений: 30 / Ошибок: 0
```

## Deviations from Plan

None - plan executed exactly as written (Option A path).

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required. The `X-Yandex-API-Key` header value is read from the existing `YandexApiOptions.ApiKey` configuration binding — no changes to appsettings.json or secrets are needed.

## Next Phase Readiness

- Phase 3 is now 4/4 plans complete (03-01 through 03-04 all have SUMMARY files)
- Phase 4 (Performance Optimization) can begin — no blockers from Phase 3

---
*Phase: 03-security-hardening*
*Completed: 2026-05-18*
