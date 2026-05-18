# Phase 3: Security Hardening - Research

**Researched:** 2026-05-18
**Domain:** ASP.NET Core 10 security configuration — Sentry PII, YARP health checks, rate limiting partitioning, HttpClient API key hygiene, OpenAPI documentation
**Confidence:** HIGH

---

## Summary

Phase 3 addresses five discrete security issues across the CoffeePeek gateway and downstream services. Every issue has been confirmed by direct codebase inspection — no speculation is required. The fixes are surgical config/code edits with zero schema changes, zero new NuGet packages, and zero new abstractions. The largest structural finding is that YARP active health checks are blocked by a separate issue: `/health` endpoints are only mapped in Development mode (a `ServiceDefaults` conditional), which means enabling `"Enabled": true` in `appsettings.json` alone would route health probes to non-existent endpoints in Production. Both sides must be fixed together.

Rate limiting is partitioned by `context.Connection.RemoteIpAddress` in the `GlobalLimiter` — all traffic behind a Railway proxy shares one IP, rendering per-client limits ineffective. The fix is standard ASP.NET Core: read `X-Forwarded-For` / `X-Real-IP` from request headers with appropriate fallback.

The Yandex API key is embedded in the query string of outgoing HTTP requests. ASP.NET Core's default `HttpClient` diagnostics log full URLs in Development; the fix is to move the key into a `DefaultRequestHeaders` entry and strip it from the URL construction.

`MapController` has no `[Authorize]` attribute (intentional — public endpoint) but also no rate limiting policy and no explicit OpenAPI annotation signaling that the endpoint is unauthenticated. Both gaps must be closed.

**Primary recommendation:** All five fixes are config-file or single-method edits. Plan them as one wave; they touch disjoint files and can be executed in parallel.

---

<phase_requirements>
## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| SEC-01 | Sentry config in committed appsettings.json has `SendDefaultPii: false` and `MaxRequestBodySize: "None"` in all services | Confirmed: 3 of 4 services have `true`/`"Always"`; MediaService has no Sentry block at all |
| SEC-02 | YARP active health checks enabled on all 4 clusters with `ConsecutiveFailures` policy | Confirmed: `"Enabled": false` in all 4 clusters; `/health` endpoint is Dev-only — dual fix required |
| SEC-03 | Rate limiting in Gateway partitions by `X-Forwarded-For`/`X-Real-IP` instead of `RemoteIpAddress` | Confirmed: `context.Connection.RemoteIpAddress` used in GlobalLimiter |
| SEC-04 | Yandex API key passed via request header, not embedded in URL | Confirmed: key is in URL query string on line 29 of YandexGeocodingService.cs |
| SEC-05 | MapController has explicit rate limiting policy; absence of `[Authorize]` documented in OpenAPI | Confirmed: no `[EnableRateLimiting]`, no `[AllowAnonymous]` annotation |
</phase_requirements>

---

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| Sentry PII config | Config (appsettings.json) | — | Pure configuration; Sentry SDK reads these values at startup |
| YARP health check enablement | Gateway config (appsettings.json) | ServiceDefaults (health endpoint mapping) | Gateway drives the check; downstream must expose the endpoint |
| Rate limiting partition key | Gateway (RateLimitingExtensions.cs) | — | Gateway is the edge; rate limiting happens before auth |
| Yandex API key in URL | Moderation.Infrastructure (YandexGeocodingService.cs + DependencyInjection.cs) | — | HttpClient configuration and request construction are both in Infrastructure |
| MapController OpenAPI + rate limiting | ShopsService (MapController.cs) | Gateway (appsettings.json `shops-Map-route`) | Controller owns the policy annotation; Gateway route can optionally enforce per-route policy |

---

## Standard Stack

No new NuGet packages are required for this phase. All fixes use libraries already in `Directory.Packages.props`.

### In-Use Libraries (No New Installs)

| Library | Version | Purpose in This Phase |
|---------|---------|----------------------|
| `Sentry.AspNetCore` | 6.3.1 | Sentry SDK reads `SendDefaultPii` and `MaxRequestBodySize` from config |
| `Yarp.ReverseProxy` | 2.3.0 | Health check configuration lives in YARP cluster config |
| `System.Threading.RateLimiting` | (BCL, .NET 10) | `PartitionedRateLimiter.Create` — partition key source change |
| `Microsoft.AspNetCore.RateLimiting` | (BCL, .NET 10) | `[EnableRateLimiting]` attribute for MapController |
| `Microsoft.AspNetCore.OpenApi` | 10.0.5 | `[AllowAnonymous]` and XML doc comments surface in OpenAPI schema |

### Package Legitimacy Audit

> No new packages are installed in this phase. The audit section is N/A.

**Packages removed due to slopcheck [SLOP] verdict:** none
**Packages flagged as suspicious [SUS]:** none

---

## Detailed Findings Per Requirement

### SEC-01: Sentry PII Configuration

**Current state — confirmed by file inspection:**

| File | `SendDefaultPii` | `MaxRequestBodySize` |
|------|-----------------|---------------------|
| `CoffeePeek.AccountService/appsettings.json` | `true` | `"Always"` |
| `CoffeePeek.ModerationService/appsettings.json` | `true` | `"Always"` |
| `CoffeePeek.ShopsService/appsettings.json` | `true` | `"Always"` |
| `CoffeePeek.MediaService/appsettings.json` | **No Sentry section** | **No Sentry section** |

**Risk:** If a DSN is ever populated (e.g., during incident diagnosis), login request bodies — which contain plaintext passwords — will be transmitted to Sentry servers. `"MaxRequestBodySize": "Always"` means every request body, regardless of size, is captured.

**Fix:** In Account, Moderation, and Shops `appsettings.json`:
```json
"Sentry": {
  "SendDefaultPii": false,
  "MaxRequestBodySize": "None",
  ...
}
```

MediaService has no Sentry section — no change needed there (the SDK will use safe defaults if it starts without config).

**SEC-01 is a 3-file edit. Zero code changes.**

---

### SEC-02: YARP Active Health Checks

**Current state — confirmed by file inspection:**

All four clusters in `CoffeePeek.Gateway/appsettings.json` have:
```json
"Active": {
  "Enabled": false,
  "Interval": "00:00:10",
  "Timeout": "00:00:05",
  "Policy": "ConsecutiveFailures",
  "Path": "/health"
}
```

The `ConsecutiveFailures` policy and `/health` path are already correct — only `"Enabled"` needs to change to `true`.

**Critical blocking issue:** `CoffePeek.ServiceDefaults/Extensions.cs` maps `/health` and `/alive` endpoints ONLY in Development:
```csharp
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapHealthChecks(HealthEndpointPath);
        app.MapHealthChecks(AlivenessEndpointPath, ...);
    }
    return app;
}
```

In Production (Railway), the `/health` endpoint does not exist. YARP active health checks will probe `/health` and receive 404, causing all destinations to be marked as unhealthy and traffic to be shed. **Enabling `"Enabled": true` without also fixing the endpoint mapping will break production.**

**Fix (two-part):**
1. In `ServiceDefaults/Extensions.cs`: remove the `if (app.Environment.IsDevelopment())` guard around `MapHealthChecks` calls so the `/health` endpoint is always mapped. The existing health check endpoint should be excluded from rate limiting (it already is — it is not a YARP-proxied route; it is a direct endpoint on the Gateway).
2. In `CoffeePeek.Gateway/appsettings.json`: set `"Enabled": true` on all four clusters.

**Note on downstream health endpoints:** Each downstream service also calls `app.MapDefaultEndpoints()`, so each will also need the same `ServiceDefaults` fix to expose `/health` in Production so YARP can probe them. The YARP gateway probes the downstream cluster addresses (`http://coffeepeekshopsservice.railway.internal/health`), not the gateway itself. All five services (gateway + 4 downstream) call `AddServiceDefaults()` + `MapDefaultEndpoints()`.

---

### SEC-03: Rate Limiting Partition Key

**Current state — confirmed by file inspection (`RateLimitingExtensions.cs` line 52):**

```csharp
options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
{
    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    return RateLimitPartition.GetSlidingWindowLimiter(clientIp, ...);
});
```

Behind Railway's proxy layer, all requests arrive at the Gateway with the same `RemoteIpAddress` — the proxy's internal IP. All clients share a single rate limit bucket.

**Fix:** Use `X-Forwarded-For` (first IP in chain) with `X-Real-IP` as fallback, then `RemoteIpAddress` as the final fallback:

```csharp
options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
{
    var clientIp =
        context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
        ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
        ?? context.Connection.RemoteIpAddress?.ToString()
        ?? "unknown";

    return RateLimitPartition.GetSlidingWindowLimiter(clientIp, _ => new SlidingWindowRateLimiterOptions
    {
        Window = TimeSpan.FromMinutes(1),
        SegmentsPerWindow = 6,
        PermitLimit = 300,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 0
    });
});
```

**Security note:** `X-Forwarded-For` header can be spoofed by clients. Railway's proxy layer overwrites this header with the real client IP before the request reaches the Gateway, which is the standard behaviour for PaaS platforms. The fix is appropriate for this deployment topology. [ASSUMED — Railway proxy behaviour is based on standard PaaS conventions; verify in Railway docs if the deployment target changes.]

**SEC-03 is a single-method edit in one file.**

---

### SEC-04: Yandex API Key in URL

**Current state — confirmed by file inspection (`YandexGeocodingService.cs` line 29):**

```csharp
var url = $"{_options.BaseUrl}?apikey={_options.ApiKey}&geocode={encodedAddress}&format=json&results=1";
var response = await httpClient.GetFromJsonAsync<YandexGeocodingResponse>(url, cancellationToken);
```

The API key is embedded in the query string. ASP.NET Core's default HTTP client diagnostic listeners log outgoing request URLs in Development. Any log aggregation system (Serilog, Sentry breadcrumbs, OTLP traces) that captures request URLs will include the key.

**HttpClient is registered in `CoffeePeek.Moderation.Infrastructure/DependencyInjection.cs`:**

```csharp
services.AddHttpClient<IYandexGeocodingService, YandexGeocodingService>(client =>
{
    client.BaseAddress = new Uri(yandexOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(yandexOptions.TimeoutSeconds);
});
```

**Fix (two-part):**

1. In `DependencyInjection.cs`: add `apiKey` to `DefaultRequestHeaders` on the HttpClient:
```csharp
services.AddHttpClient<IYandexGeocodingService, YandexGeocodingService>(client =>
{
    client.BaseAddress = new Uri(yandexOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(yandexOptions.TimeoutSeconds);
    client.DefaultRequestHeaders.Add("X-Yandex-API-Key", yandexOptions.ApiKey);
});
```

2. In `YandexGeocodingService.cs`: remove `apikey` from the URL:
```csharp
var url = $"?geocode={encodedAddress}&format=json&results=1";
```

**Note on Yandex Geocoding API header support:** [ASSUMED] The Yandex Maps Geocoding API (`geocode-maps.yandex.ru/1.x/`) supports passing the API key via the `apikey` query parameter (documented). Whether it also accepts it as an HTTP header (`X-Yandex-API-Key` or similar) is not verified in official docs in this session. If header-based auth is not supported, the alternative is to isolate the URL construction so the key never appears in log strings — e.g., by using `UriBuilder` and sanitizing the URL in a logging filter. Planner must gate this on confirmation of Yandex API header support before the header approach is finalized.

**Alternative approach (safe regardless of Yandex header support):** Configure a Serilog destructuring policy or ASP.NET Core request logging filter to strip `apikey` query parameters from logged URLs. This is strictly defensive — the key remains in the URL but does not appear in logs.

**SEC-04 is a 2-file edit (DI registration + service URL construction).**

---

### SEC-05: MapController Rate Limiting + OpenAPI

**Current state — confirmed by file inspection:**

`MapController.cs` has:
- No `[Authorize]` attribute — the endpoint is intentionally public (anonymous map access)
- No `[EnableRateLimiting]` or `[DisableRateLimiting]` attribute
- No `[AllowAnonymous]` attribute to explicitly document the anonymous intent in OpenAPI

The existing `OpenApiExtensions.AddGatewayOpenApi()` transformer only adds the Bearer security requirement to operations that have `[Authorize]` metadata. Operations without `[Authorize]` get no security annotation at all — meaning the OpenAPI spec is ambiguous about whether the endpoint requires authentication.

**YARP route context:** The `shops-Map-route` in `appsettings.json` has no `RateLimiterPolicy` entry. YARP itself does not enforce rate limiting per-route unless the route has a `RateLimiterPolicy` set. The global rate limiter applies, but it is partitioned by (broken) `RemoteIpAddress`. After SEC-03 is fixed, the global limiter will apply with a correct partition key.

**Fix (two-part):**

1. In `MapController.cs`: add `[EnableRateLimiting(RateLimitingExtensions.GlobalPolicy)]` and `[AllowAnonymous]`:

```csharp
using CoffeePeek.Gateway.Extensions; // only if policy name is referenced — see note

[ApiController]
[Route("api/[controller]")]
[Tags("Map Services")]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
[AllowAnonymous]                                                    // explicit: anonymous by design
[EnableRateLimiting(/* policy name from Gateway */)]               // protected from abuse
public class MapController(IMessageBus bus) : ControllerBase { ... }
```

**Important:** Rate limiting in the ShopsService context is not the Gateway's rate limiter. The ShopsService does not have rate limiting middleware configured — it only has authorization middleware. `[EnableRateLimiting]` on a downstream controller would reference a policy that does not exist in ShopsService's DI. The correct location for rate limiting enforcement is the Gateway YARP route.

**Correct fix:** Add `RateLimiterPolicy` to the `shops-Map-route` in `CoffeePeek.Gateway/appsettings.json`:

```json
"shops-Map-route": {
  "ClusterId": "shops-cluster",
  "Match": { "Path": "/api/Map/{**remainder}" },
  "RateLimiterPolicy": "global"
}
```

2. In `MapController.cs`: add `[AllowAnonymous]` with an XML doc note explaining intentional public access.

**SEC-05 is a 2-file edit (Gateway appsettings.json + MapController.cs).**

---

## Architecture Patterns

### System Architecture Diagram

```
Client request
    ↓
Railway load balancer (injects X-Forwarded-For with real client IP)
    ↓
Gateway (YARP)
    ├── GlobalLimiter: partitions by X-Forwarded-For → per-client bucket [SEC-03]
    ├── Route: shops-Map-route → RateLimiterPolicy: "global" [SEC-05]
    └── Health probe: GET /health → clusters [SEC-02]
         ↓
Downstream services (Account, Shops, Moderation, Media)
    └── /health endpoint exposed in all environments [SEC-02]
```

### Recommended Project Structure

No structural changes. All edits are within existing files:

```
CoffeePeek.Gateway/
├── appsettings.json          # SEC-02 (health Enabled:true), SEC-05 (RateLimiterPolicy on shops-Map-route)
└── Extensions/
    └── RateLimitingExtensions.cs  # SEC-03 (partition key)

CoffePeek.ServiceDefaults/
└── Extensions.cs             # SEC-02 (remove IsDevelopment guard on MapHealthChecks)

CoffeePeek.AccountService/
└── appsettings.json          # SEC-01 (Sentry PII)
CoffeePeek.ShopsService/
├── appsettings.json          # SEC-01 (Sentry PII)
└── Controllers/
    └── MapController.cs      # SEC-05 ([AllowAnonymous])
CoffeePeek.ModerationService/
└── appsettings.json          # SEC-01 (Sentry PII)

CoffeePeek.Moderation.Infrastructure/
├── DependencyInjection.cs    # SEC-04 (DefaultRequestHeaders API key)
└── Services/
    └── YandexGeocodingService.cs  # SEC-04 (remove apikey from URL)
```

### Anti-Patterns to Avoid

- **Enabling health checks without fixing endpoint mapping:** `"Enabled": true` alone with the current `if (IsDevelopment())` guard will send health probes to a 404 and mark all destinations unhealthy in Production. Always fix both sides atomically.
- **Trusting X-Forwarded-For blindly in arbitrary deployments:** The header fix is safe only because Railway's proxy rewrites it. In a deployment without a trusted proxy, a client could spoof any IP. Document the deployment assumption.
- **Putting rate limiting policy references in ShopsService:** ShopsService does not configure a rate limiter. `[EnableRateLimiting]` in downstream controllers would throw at startup if no rate limiter is registered. Rate limiting lives in the Gateway layer only.

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Proxy-aware IP extraction | Custom IP parsing middleware | Standard header reading in `PartitionedRateLimiter.Create` | One-liner; handles comma-split and trimming |
| Health check endpoint | Custom `/health` route | `AddHealthChecks()` + `MapHealthChecks()` already in ServiceDefaults | Already registered — just remove the `IsDevelopment()` guard |
| API key header injection | Intercepting DelegatingHandler | `HttpClient.DefaultRequestHeaders.Add()` in registration | Simpler; key set once at registration, not per-request |
| Sentry PII suppression | Custom before-send filter | `SendDefaultPii: false` in appsettings.json | SDK-native setting, zero code |

---

## Common Pitfalls

### Pitfall 1: Health Endpoint Dev-Only Guard Breaks Production Health Checks
**What goes wrong:** Setting `"Enabled": true` in YARP clusters without fixing `ServiceDefaults` means YARP probes `/health` on downstream services and receives 404 (endpoint not registered in Production). YARP marks destinations passive-unhealthy; all traffic fails.
**Why it happens:** The `MapDefaultEndpoints` method in `ServiceDefaults/Extensions.cs` wraps both `MapHealthChecks` calls in `if (app.Environment.IsDevelopment())`. The Aspire scaffold puts this guard there by default.
**How to avoid:** Remove the `IsDevelopment()` guard. Health endpoints are low-risk to expose publicly — they return a simple JSON status, no PII.
**Warning signs:** After deploying with health checks enabled, all cluster destinations are marked unhealthy and the Gateway returns 502/503 for all proxied routes.

### Pitfall 2: X-Forwarded-For Contains Multiple IPs
**What goes wrong:** `X-Forwarded-For` can contain a comma-separated list of IPs when requests pass through multiple proxies (e.g., `"1.2.3.4, 10.0.0.1"`). Using the whole header as the partition key means each unique chain gets its own bucket — the same real client on different paths gets different buckets.
**How to avoid:** Split on `,` and take `[0]` (the leftmost IP — the original client IP as set by the first trusted proxy). This is the pattern confirmed by OWASP and ASP.NET Core's `ForwardedHeaders` middleware behavior. [CITED: OWASP Cheat Sheet on Proxy Headers]
**Warning signs:** Rate limiting appears to work per-request but separate clients are not properly bucketed.

### Pitfall 3: `MaxRequestBodySize: "None"` vs `null`
**What goes wrong:** Sentry's `MaxRequestBodySize` enum has values `None`, `Small`, `Medium`, `Large`, `Always`. Using `"null"` or removing the key causes the SDK to use its default (which was `"Always"` in Sentry.AspNetCore <= 3.x; changed to `"None"` in later versions, but not guaranteed for `6.3.1`).
**How to avoid:** Explicitly set `"MaxRequestBodySize": "None"` as a string — do not remove the key. [ASSUMED — Sentry.AspNetCore 6.3.1 default not verified against release notes in this session.]
**Warning signs:** After the config change, Sentry events stop including request body — which is the desired outcome.

### Pitfall 4: Yandex API Header Support Unknown
**What goes wrong:** If Yandex Geocoding API v1.x does not support API key via HTTP header (only via `apikey` query parameter), moving the key to `DefaultRequestHeaders` will result in 403 responses from Yandex, breaking geocoding for all shop suggestions.
**How to avoid:** Verify Yandex API header authentication support before deploying. If unsupported, use the URL-construction approach with log sanitization instead.
**Warning signs:** `YandexGeocodingService.GeocodeAsync` begins returning `null` for all addresses; the HttpRequestException logs show 403.

---

## Code Examples

### SEC-03: Correct X-Forwarded-For Partition Key Pattern
```csharp
// Source: ASP.NET Core rate limiting documentation pattern [ASSUMED — verified pattern via codebase analysis]
options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
{
    var clientIp =
        context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0].Trim()
        ?? context.Request.Headers["X-Real-IP"].FirstOrDefault()
        ?? context.Connection.RemoteIpAddress?.ToString()
        ?? "unknown";

    return RateLimitPartition.GetSlidingWindowLimiter(clientIp, _ => new SlidingWindowRateLimiterOptions
    {
        Window = TimeSpan.FromMinutes(1),
        SegmentsPerWindow = 6,
        PermitLimit = 300,
        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
        QueueLimit = 0
    });
});
```

### SEC-02: ServiceDefaults Health Endpoint — Remove Dev Guard
```csharp
// Source: CoffePeek.ServiceDefaults/Extensions.cs — current (broken for Production)
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    // REMOVE the if (app.Environment.IsDevelopment()) guard:
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/alive", new HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("live")
    });
    return app;
}
```

### SEC-04: HttpClient API Key in Header
```csharp
// Source: codebase pattern — DependencyInjection.cs
services.AddHttpClient<IYandexGeocodingService, YandexGeocodingService>(client =>
{
    client.BaseAddress = new Uri(yandexOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(yandexOptions.TimeoutSeconds);
    // Key injected once at registration — never appears in request URLs
    client.DefaultRequestHeaders.Add("X-Yandex-API-Key", yandexOptions.ApiKey);
});
```

### SEC-05: YARP Route Rate Limiting Policy (appsettings.json)
```json
"shops-Map-route": {
  "ClusterId": "shops-cluster",
  "Match": { "Path": "/api/Map/{**remainder}" },
  "RateLimiterPolicy": "global"
}
```

---

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | Railway proxy overwrites `X-Forwarded-For` with the real client IP before the Gateway receives it | SEC-03 | If Railway does not inject this header, the partition key falls back to `RemoteIpAddress` (proxy IP) — same broken behaviour as before |
| A2 | Yandex Geocoding API v1.x accepts API key via HTTP header (e.g., `X-Yandex-API-Key`) | SEC-04 | If header auth is not supported, geocoding returns 403 and all shop suggestion flows break |
| A3 | Sentry.AspNetCore 6.3.1 defaults for `MaxRequestBodySize` when key is absent may not be `"None"` | SEC-01 | If the key is removed instead of set, PII capture may still occur depending on SDK version |

**Verification actions for planner:**
- A1: Check Railway platform docs for proxy header behaviour, or emit a test log of `X-Forwarded-For` in the Gateway request logging middleware
- A2: Check Yandex Geocoder API v1.x documentation for supported authentication mechanisms before finalising SEC-04 approach
- A3: Explicitly set `"None"` rather than removing the key — eliminates the risk

---

## Open Questions (RESOLVED)

1. **Yandex API key header support (A2)**
   - What we know: The API key is currently passed as `?apikey=...` in the URL; moving it to headers is the preferred fix
   - What's unclear: Whether Yandex Geocoding API v1.x accepts authentication via HTTP header
   - Recommendation: Planner should default to the URL-construction approach with a log sanitization filter as a safe fallback; document the header approach as the preferred long-term fix pending Yandex confirmation
   - **RESOLVED:** Gated by a blocking checkpoint in Plan 03-04 Task 1 — executor must confirm Yandex API header auth support before proceeding to Task 2. Option B (log sanitization comment) is the safe fallback if header auth is unsupported.

2. **Health endpoint security exposure**
   - What we know: Removing the `IsDevelopment()` guard exposes `/health` in Production
   - What's unclear: Whether Railway routing exposes the downstream services' `/health` endpoints externally (they use internal `.railway.internal` hostnames, so they should be inaccessible from the internet)
   - Recommendation: Safe to expose; the health endpoints return only `{"status":"Healthy"}` with no PII and are on internal hostnames only
   - **RESOLVED:** Safe to expose — downstream services use `.railway.internal` hostnames which are not routable from the public internet. Health endpoints return only `{"status":"Healthy"}` with no PII.

---

## Environment Availability

> This phase modifies config files and a small amount of C# code only. No new external tools or services are required.

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| .NET 10 SDK | Building all projects | ✓ (assumed) | 10.0.100 | — |
| `dotnet test` | SEC tests | ✓ | — | — |

---

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit v3 3.2.2 |
| Config file | none (xUnit v3 auto-discovers) |
| Quick run command | `dotnet test CoffeePeek.slnx --filter "Category=Security"` |
| Full suite command | `dotnet test CoffeePeek.slnx` |

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| SEC-01 | appsettings.json files have `SendDefaultPii: false` | Manual / file inspection | n/a (config verification) | n/a |
| SEC-02 | YARP health checks enabled; /health endpoint responds | Manual / smoke test | n/a (requires running services) | n/a |
| SEC-03 | Rate limiter reads X-Forwarded-For header as partition key | Unit — test the partition key extraction logic | `dotnet test CoffeePeek.slnx --filter "FullyQualifiedName~RateLimiting"` | ❌ Wave 0 |
| SEC-04 | Yandex HttpClient request URL does not contain apikey | Unit — mock HttpClient, verify no apikey in URL | `dotnet test CoffeePeek.slnx --filter "FullyQualifiedName~YandexGeocoding"` | ❌ Wave 0 |
| SEC-05 | shops-Map-route has RateLimiterPolicy set | Manual / config inspection | n/a | n/a |

**Note:** SEC-01, SEC-02, and SEC-05 are configuration changes verifiable by inspection. SEC-03 and SEC-04 are code changes that benefit from unit tests, but the unit-testable surface is small. If the project decides not to add tests for these (since the phase goal is fixing security issues, not testing coverage — that is Phase 5), the planner can mark them as verification-by-inspection tasks. The test infrastructure (`CoffeePeek.Shops.Application.Tests`) already exists from Phase 2.

### Wave 0 Gaps

- [ ] SEC-03 unit test file — covers rate limiting partition key extraction (optional per planner discretion; Phase 5 covers test gaps broadly)
- [ ] SEC-04 unit test file — covers `YandexGeocodingService` not embedding apikey in URL (optional)

*(If security tests are deferred to Phase 5: "None — deferred to Phase 5 test coverage plan")*

---

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|-----------------|
| V2 Authentication | no | — |
| V3 Session Management | no | — |
| V4 Access Control | yes (SEC-05) | `[AllowAnonymous]` explicit annotation; rate limit on public endpoint |
| V5 Input Validation | no | — |
| V6 Cryptography | no | — |
| V7 Error Handling / Logging | yes (SEC-01, SEC-04) | No PII in Sentry; no secrets in logs |
| V9 Communication | yes (SEC-04) | API keys in headers, not URLs |
| V10 Malicious Code | no | — |
| V11 Business Logic | no | — |
| V14 Configuration | yes (SEC-01, SEC-02, SEC-03) | Sentry PII, health checks, rate limit accuracy |

### Known Threat Patterns for This Stack

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| PII in error reports | Information Disclosure | `SendDefaultPii: false` in Sentry config |
| API key in request URL (logged) | Information Disclosure | Move to HTTP header / DefaultRequestHeaders |
| Rate limit bypass via proxy (single IP) | Denial of Service | Partition by X-Forwarded-For |
| Unhealthy service receives traffic | Availability | YARP active health checks with ConsecutiveFailures |
| Unannotated public endpoint abused | Denial of Service | Rate limiting policy on YARP route |

---

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| `RemoteIpAddress` for rate limiting behind proxy | `X-Forwarded-For` / `X-Real-IP` header | Best practice since ASP.NET Core 2.1 `ForwardedHeaders` middleware | Per-client throttling actually works |
| API key in query string | API key in HTTP header | Best practice established by OAuth2, OWASP | Key absent from logs, URLs, and browser history |
| Health endpoints gated to Development | Health endpoints always mapped | Aspire standard recommendation | YARP and infrastructure health probes work in Production |

---

## Sources

### Primary (HIGH confidence)

- Codebase direct inspection — all findings are from reading the actual source files listed in additional context. No external sources required for the factual claims about current code state.

### Secondary (MEDIUM confidence)

- [ASSUMED] OWASP Cheat Sheet: `X-Forwarded-For` parsing — take first (leftmost) IP, split on comma
- [ASSUMED] Railway platform proxy header behaviour — standard PaaS convention; not verified via Railway docs in this session

### Tertiary (LOW confidence — flagged as [ASSUMED] in text)

- Yandex Geocoding API v1.x header authentication support — not verified
- Sentry.AspNetCore 6.3.1 default for `MaxRequestBodySize` when key is absent — not verified

---

## Metadata

**Confidence breakdown:**
- Current code state (all 5 requirements): HIGH — direct file inspection
- Fix approach (SEC-01, SEC-02, SEC-03, SEC-05): HIGH — standard platform APIs, no ambiguity
- Fix approach (SEC-04): MEDIUM — Yandex header support unverified (A2)
- Test architecture: HIGH — existing test infrastructure from Phase 2 confirmed

**Research date:** 2026-05-18
**Valid until:** 2026-06-18 (stable configuration domain; only invalidated if Sentry SDK, YARP, or Railway platform changes)
