# Phase 3: Security Hardening - Pattern Map

**Mapped:** 2026-05-18
**Files analyzed:** 9 files (7 modified, 2 context analogs)
**Analogs found:** 9 / 9

---

## File Classification

| New/Modified File | Role | Data Flow | Closest Analog | Match Quality |
|-------------------|------|-----------|----------------|---------------|
| `CoffeePeek.AccountService/appsettings.json` | config | request-response | `CoffeePeek.ModerationService/appsettings.json` | exact (same Sentry block) |
| `CoffeePeek.ShopsService/appsettings.json` | config | request-response | `CoffeePeek.AccountService/appsettings.json` | exact (same Sentry block) |
| `CoffeePeek.ModerationService/appsettings.json` | config | request-response | `CoffeePeek.AccountService/appsettings.json` | exact (same Sentry block) |
| `CoffeePeek.Gateway/appsettings.json` | config | request-response | self (existing cluster blocks) | exact (pattern already present) |
| `CoffeePeek.Gateway/Extensions/RateLimitingExtensions.cs` | middleware | request-response | self (existing GlobalLimiter) | exact (single-method edit) |
| `CoffeePeek.Moderation.Infrastructure/Services/YandexGeocodingService.cs` | service | request-response | self (existing GeocodeAsync) | exact (single-line edit) |
| `CoffeePeek.Moderation.Infrastructure/DependencyInjection.cs` | config/DI | request-response | self (existing AddHttpClient block) | exact (single-line addition) |
| `CoffeePeek.ShopsService/Controllers/MapController.cs` | controller | request-response | `CoffeePeek.ShopsService/Controllers/CoffeeShopsController.cs` | role-match (same project, same controller pattern) |
| `CoffePeek.ServiceDefaults/Extensions.cs` | utility | request-response | self (existing MapDefaultEndpoints) | exact (remove conditional guard) |

---

## Pattern Assignments

### SEC-01: `CoffeePeek.AccountService/appsettings.json` (config, SEC-01)

**Analog:** `CoffeePeek.AccountService/appsettings.json` lines 9–18 (current, broken state)

**Current state** (lines 9–18):
```json
"Sentry": {
  "Dsn": "",
  "SendDefaultPii": true,
  "MaxRequestBodySize": "Always",
  "MinimumBreadcrumbLevel": "Debug",
  "MinimumEventLevel": "Warning",
  "AttachStackTrace": true,
  "Debug": false,
  "DiagnosticLevel": "Error"
}
```

**Required state — copy this block exactly:**
```json
"Sentry": {
  "Dsn": "",
  "SendDefaultPii": false,
  "MaxRequestBodySize": "None",
  "MinimumBreadcrumbLevel": "Debug",
  "MinimumEventLevel": "Warning",
  "AttachStackTrace": true,
  "Debug": false,
  "DiagnosticLevel": "Error"
}
```

**Two fields change:** `"SendDefaultPii": true` → `false`; `"MaxRequestBodySize": "Always"` → `"None"`. All other keys remain unchanged. The `"Debug"` value in AccountService is `false` — preserve the per-file value; do not unify across services.

---

### SEC-01: `CoffeePeek.ShopsService/appsettings.json` (config, SEC-01)

**Analog:** `CoffeePeek.ShopsService/appsettings.json` lines 20–30 (current, broken state)

**Current state** (lines 20–29):
```json
"Sentry": {
  "Dsn": "",
  "SendDefaultPii": true,
  "MaxRequestBodySize": "Always",
  "MinimumBreadcrumbLevel": "Debug",
  "MinimumEventLevel": "Warning",
  "AttachStackTrace": true,
  "Debug": true,
  "DiagnosticLevel": "Error"
}
```

**Required state:**
```json
"Sentry": {
  "Dsn": "",
  "SendDefaultPii": false,
  "MaxRequestBodySize": "None",
  "MinimumBreadcrumbLevel": "Debug",
  "MinimumEventLevel": "Warning",
  "AttachStackTrace": true,
  "Debug": true,
  "DiagnosticLevel": "Error"
}
```

Note: `"Debug": true` is the existing value in ShopsService — preserve it.

---

### SEC-01: `CoffeePeek.ModerationService/appsettings.json` (config, SEC-01)

**Analog:** `CoffeePeek.ModerationService/appsettings.json` lines 9–18 (current, broken state)

**Current state** (lines 9–18):
```json
"Sentry": {
  "Dsn": "",
  "SendDefaultPii": true,
  "MaxRequestBodySize": "Always",
  "MinimumBreadcrumbLevel": "Debug",
  "MinimumEventLevel": "Warning",
  "AttachStackTrace": true,
  "Debug": true,
  "DiagnosticLevel": "Error"
}
```

**Required state:**
```json
"Sentry": {
  "Dsn": "",
  "SendDefaultPii": false,
  "MaxRequestBodySize": "None",
  "MinimumBreadcrumbLevel": "Debug",
  "MinimumEventLevel": "Warning",
  "AttachStackTrace": true,
  "Debug": true,
  "DiagnosticLevel": "Error"
}
```

---

### SEC-02 (part A): `CoffeePeek.Gateway/appsettings.json` (config, SEC-02)

**Analog:** `CoffeePeek.Gateway/appsettings.json` lines 107–172 (existing cluster blocks, all four clusters)

**Current state for every cluster** (representative — `account-cluster`, lines 108–122):
```json
"account-cluster": {
  "HealthCheck": {
    "Active": {
      "Enabled": false,
      "Interval": "00:00:10",
      "Timeout": "00:00:05",
      "Policy": "ConsecutiveFailures",
      "Path": "/health"
    }
  },
  "Destinations": {
    "destination1": {
      "Address": "http://coffeepeekaccountservice.railway.internal"
    }
  }
}
```

**Required state — change `"Enabled": false` to `"Enabled": true` in all four clusters:**
```json
"Active": {
  "Enabled": true,
  "Interval": "00:00:10",
  "Timeout": "00:00:05",
  "Policy": "ConsecutiveFailures",
  "Path": "/health"
}
```

Apply to: `account-cluster` (line 111), `shops-cluster` (line 127), `moderation-cluster` (line 143), `media-cluster` (line 159). All other cluster values (Interval, Timeout, Policy, Path, Destination Address) remain unchanged.

**ALSO in this file — SEC-05 RateLimiterPolicy on shops-Map-route:**

**Current state** (lines 55–58):
```json
"shops-Map-route": {
  "ClusterId": "shops-cluster",
  "Match": { "Path": "/api/Map/{**remainder}" }
}
```

**Required state:**
```json
"shops-Map-route": {
  "ClusterId": "shops-cluster",
  "Match": { "Path": "/api/Map/{**remainder}" },
  "RateLimiterPolicy": "global"
}
```

The string `"global"` must match `RateLimitingExtensions.GlobalPolicy` constant (`"global"`, line 9 of `RateLimitingExtensions.cs`).

---

### SEC-02 (part B): `CoffePeek.ServiceDefaults/Extensions.cs` (utility, SEC-02)

**Analog:** `CoffePeek.ServiceDefaults/Extensions.cs` lines 96–109 (current, broken state)

**Current state** (lines 96–109):
```csharp
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.MapHealthChecks(HealthEndpointPath);

        app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });
    }

    return app;
}
```

**Required state — remove the `if (app.Environment.IsDevelopment())` guard entirely:**
```csharp
public static WebApplication MapDefaultEndpoints(this WebApplication app)
{
    app.MapHealthChecks(HealthEndpointPath);

    app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
    {
        Predicate = r => r.Tags.Contains("live")
    });

    return app;
}
```

Constants `HealthEndpointPath = "/health"` and `AlivenessEndpointPath = "/alive"` (lines 20–21) are already defined — do not duplicate them. The `AddDefaultHealthChecks` registration (lines 87–93) is already unconditional and does not need to change.

---

### SEC-03: `CoffeePeek.Gateway/Extensions/RateLimitingExtensions.cs` (middleware, SEC-03)

**Analog:** `CoffeePeek.Gateway/Extensions/RateLimitingExtensions.cs` lines 50–61 (current, broken state)

**Current state** (lines 50–61):
```csharp
// Use client IP as the partition key for all policies
options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
{
    var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
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

**Required state — replace only the `clientIp` extraction line (line 52):**
```csharp
// Use client IP as the partition key — reads X-Forwarded-For (Railway proxy injects real client IP)
// with X-Real-IP and RemoteIpAddress as fallbacks
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

No new `using` directives are required — `System.Threading.RateLimiting` and `Microsoft.AspNetCore.RateLimiting` are already imported (lines 1–2). No other lines in the method change.

---

### SEC-04 (part A): `CoffeePeek.Moderation.Infrastructure/DependencyInjection.cs` (config/DI, SEC-04)

**Analog:** `CoffeePeek.Moderation.Infrastructure/DependencyInjection.cs` lines 12–17 (current, broken state)

**Current state** (lines 12–17):
```csharp
services.AddHttpClient<IYandexGeocodingService, YandexGeocodingService>(client =>
{
    client.BaseAddress = new Uri(yandexOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(yandexOptions.TimeoutSeconds);
});
```

**Required state — add `DefaultRequestHeaders` entry after `Timeout`:**
```csharp
services.AddHttpClient<IYandexGeocodingService, YandexGeocodingService>(client =>
{
    client.BaseAddress = new Uri(yandexOptions.BaseUrl);
    client.Timeout = TimeSpan.FromSeconds(yandexOptions.TimeoutSeconds);
    // API key in header — never appears in request URLs or logs
    client.DefaultRequestHeaders.Add("X-Yandex-API-Key", yandexOptions.ApiKey);
});
```

No new `using` directives needed — `HttpClient.DefaultRequestHeaders` is in `System.Net.Http` (implicitly available via `ImplicitUsings`). No other lines in the file change.

**ASSUMPTION GATE (A2):** This pattern assumes Yandex Geocoding API v1.x accepts `X-Yandex-API-Key` as a request header. If header auth is not supported by Yandex, the executor must use the alternative approach: keep the key in the URL but add a Serilog destructuring policy to strip `apikey` query parameters from logged request URLs. Planner must flag this gate.

---

### SEC-04 (part B): `CoffeePeek.Moderation.Infrastructure/Services/YandexGeocodingService.cs` (service, SEC-04)

**Analog:** `CoffeePeek.Moderation.Infrastructure/Services/YandexGeocodingService.cs` lines 28–31 (current, broken state)

**Current state** (lines 28–31):
```csharp
var encodedAddress = Uri.EscapeDataString(address);
var url = $"{_options.BaseUrl}?apikey={_options.ApiKey}&geocode={encodedAddress}&format=json&results=1";

var response = await httpClient.GetFromJsonAsync<YandexGeocodingResponse>(url, cancellationToken);
```

**Required state — remove `apikey` from URL (header carries it now via DI registration):**
```csharp
var encodedAddress = Uri.EscapeDataString(address);
var url = $"?geocode={encodedAddress}&format=json&results=1";

var response = await httpClient.GetFromJsonAsync<YandexGeocodingResponse>(url, cancellationToken);
```

`_options.BaseUrl` is already set as `client.BaseAddress` in the DI registration — the relative URL `?geocode=...` resolves correctly against the base. `_options.ApiKey` can be removed from `YandexApiOptions` usage in this file (the field `_options` is still needed for `_options.BaseUrl` only, but after the DI fix `BaseUrl` is also in `BaseAddress`, so the field becomes unused — leave it as-is unless clean-up is in scope).

No import changes needed.

---

### SEC-05: `CoffeePeek.ShopsService/Controllers/MapController.cs` (controller, SEC-05)

**Analog (contrast — authenticated controller):** `CoffeePeek.ShopsService/Controllers/CoffeeShopsController.cs` lines 1–17

**Imports pattern from CoffeeShopsController** (lines 1–10):
```csharp
using System.ComponentModel.DataAnnotations;
using CoffeePeek.Shared.Auth;
using CoffeePeek.Shared.Kernel.Response;
using CoffeePeek.Shops.Application.Common.Responses;
using CoffeePeek.Shops.Application.Features.CoffeeShop.GetCoffeeShop;
using CoffeePeek.Shops.Application.Features.CoffeeShop.SearchCoffeeShops;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
```

**Current state of MapController.cs class declaration** (lines 9–14):
```csharp
[ApiController]
[Route("api/[controller]")]
[Tags("Map Services")]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
public class MapController(IMessageBus bus) : ControllerBase
```

**Required state — add `[AllowAnonymous]` attribute and its using:**

Add to imports (line 1 area):
```csharp
using Microsoft.AspNetCore.Authorization;
```

Change class declaration:
```csharp
[ApiController]
[Route("api/[controller]")]
[Tags("Map Services")]
[Produces("application/json")]
[ProducesErrorResponseType(typeof(ErrorResponse))]
[AllowAnonymous] // This endpoint is intentionally public — no authentication required
public class MapController(IMessageBus bus) : ControllerBase
```

**OpenAPI surface:** The `BearerSecurityTransformer` in `CoffeePeek.Shared.Web/BearerSecurityTransformer.cs` (lines 29–35) already checks `!api.ActionDescriptor.EndpointMetadata.OfType<IAllowAnonymous>().Any()` to exclude anonymous endpoints from Bearer requirements. Adding `[AllowAnonymous]` is sufficient to make the OpenAPI spec correctly omit the Bearer security requirement for this endpoint — no additional OpenAPI code change is needed.

**Rate limiting:** Do NOT add `[EnableRateLimiting]` to MapController — ShopsService has no rate limiter registered in its DI. Rate limiting for this route is enforced at the Gateway layer via `"RateLimiterPolicy": "global"` on `shops-Map-route` (covered in the Gateway appsettings.json section above).

---

## Shared Patterns

### Sentry Config Block
**Source:** `CoffeePeek.AccountService/appsettings.json` lines 9–18 (reference state, before fix)
**Apply to:** AccountService, ShopsService, ModerationService appsettings.json
**Safe values to use in all three:**
```json
"SendDefaultPii": false,
"MaxRequestBodySize": "None"
```
All other Sentry keys (`Dsn`, `MinimumBreadcrumbLevel`, `MinimumEventLevel`, `AttachStackTrace`, `Debug`, `DiagnosticLevel`) must preserve their per-file values — do not unify `"Debug"` values across services.

### HttpClient Registration with DefaultRequestHeaders
**Source:** `CoffeePeek.Moderation.Infrastructure/DependencyInjection.cs` lines 12–17 (after fix)
**Apply to:** Any future typed HttpClient registrations that require API key authentication
**Pattern:** Set secret headers in the DI registration lambda (`client.DefaultRequestHeaders.Add()`), never in the service method that constructs the URL.

### Health Endpoint Always-On
**Source:** `CoffePeek.ServiceDefaults/Extensions.cs` lines 96–109 (after fix)
**Apply to:** All five services (Gateway + 4 downstream) — all call `MapDefaultEndpoints()`
**Pattern:** `MapHealthChecks` calls are unconditional — no `IsDevelopment()` guard. Health endpoints return only status JSON with no PII and run on internal Railway hostnames only.

### Rate Limiting Partition Key (Proxy-Aware)
**Source:** `CoffeePeek.Gateway/Extensions/RateLimitingExtensions.cs` lines 50–61 (after fix)
**Apply to:** Gateway GlobalLimiter only — downstream services have no rate limiting
**Pattern:** `X-Forwarded-For` first (split on comma, take index 0) → `X-Real-IP` → `RemoteIpAddress` → `"unknown"`. Document the Railway proxy assumption in a comment.

### YARP Route Rate Limiting
**Source:** `CoffeePeek.Gateway/appsettings.json` routes section (pattern: existing routes without `RateLimiterPolicy`)
**Apply to:** Any YARP route that should have an explicit rate limit policy
**Pattern:** Add `"RateLimiterPolicy": "<policy-name>"` as a sibling of `"ClusterId"` and `"Match"` in the route JSON object. Policy name must match a constant defined in `RateLimitingExtensions` (`"global"`, `"auth-endpoints"`, `"media-upload"`).

---

## No Analog Found

No files in this phase lack a codebase analog. All eight target files are either self-analogs (direct edits to existing files) or have a close role-match within the same service.

---

## Metadata

**Analog search scope:** All `Controllers/`, `Extensions/`, `Services/`, `DependencyInjection.cs` files; all `appsettings.json` files across all services; `CoffePeek.ServiceDefaults/Extensions.cs`
**Files scanned:** 12 source files read directly; 2 Grep searches for `AllowAnonymous` and `EnableRateLimiting`
**Key constraint:** `[EnableRateLimiting]` has zero existing usages in the codebase — it must not be applied to downstream controllers. Rate limiting is Gateway-only.
**Pattern extraction date:** 2026-05-18
