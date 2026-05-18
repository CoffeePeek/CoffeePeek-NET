---
phase: 03-security-hardening
plan: "01"
status: complete
completed_at: "2026-05-18"
requirements_closed:
  - SEC-01
subsystem: configuration
tags: [security, sentry, pii, config]
dependency_graph:
  requires: []
  provides: [sentry-pii-safe-defaults]
  affects: [CoffeePeek.AccountService, CoffeePeek.ShopsService, CoffeePeek.ModerationService]
tech_stack:
  added: []
  patterns: [safe-sentry-defaults]
key_files:
  modified:
    - CoffeePeek.AccountService/appsettings.json
    - CoffeePeek.ShopsService/appsettings.json
    - CoffeePeek.ModerationService/appsettings.json
decisions:
  - "MaxRequestBodySize set to None (not Small/Medium) to prevent any body capture regardless of request size"
metrics:
  duration_minutes: 5
  tasks_completed: 1
  files_changed: 3
---

# Phase 03 Plan 01: Sentry PII Configuration Fix Summary

**One-liner:** Disabled Sentry PII capture (`SendDefaultPii: false`, `MaxRequestBodySize: None`) across Account, Shops, and Moderation service configs, preventing plaintext passwords from being exfiltrated if a DSN is ever populated.

## Objective

Fix the latent data-exfiltration risk where committed `appsettings.json` files had `SendDefaultPii: true` and `MaxRequestBodySize: "Always"`. With those settings, connecting a Sentry DSN — even temporarily for incident investigation — would cause the SDK to transmit full request bodies (including login bodies with plaintext passwords) to sentry.io.

## Changes Made

| File | Change |
|------|--------|
| `CoffeePeek.AccountService/appsettings.json` | `SendDefaultPii: false`, `MaxRequestBodySize: None` |
| `CoffeePeek.ShopsService/appsettings.json` | `SendDefaultPii: false`, `MaxRequestBodySize: None` |
| `CoffeePeek.ModerationService/appsettings.json` | `SendDefaultPii: false`, `MaxRequestBodySize: None` |

Preserved per-file `Debug` values: AccountService `false`, ShopsService `true`, ModerationService `true`.

## Verification

**grep output (all 6 lines confirm corrected values):**
```
CoffeePeek.AccountService/appsettings.json:11:    "SendDefaultPii": false,
CoffeePeek.AccountService/appsettings.json:12:    "MaxRequestBodySize": "None",
CoffeePeek.ShopsService/appsettings.json:22:    "SendDefaultPii": false,
CoffeePeek.ShopsService/appsettings.json:23:    "MaxRequestBodySize": "None",
CoffeePeek.ModerationService/appsettings.json:11:    "SendDefaultPii": false,
CoffeePeek.ModerationService/appsettings.json:12:    "MaxRequestBodySize": "None",
```

**Build result:** `Предупреждений: 191, Ошибок: 0` — Build succeeded, 0 errors (191 pre-existing warnings unrelated to this change).

## Commits

| Task | Commit | Files |
|------|--------|-------|
| Task 1: Disable Sentry PII capture | `5677280` | AccountService/appsettings.json, ShopsService/appsettings.json, ModerationService/appsettings.json |

## Deviations

None — plan executed exactly as written.

## Known Stubs

None.

## Threat Flags

None — this plan eliminates T-03-01 and T-03-02 from the threat register by setting safe defaults in all committed configs.

## Self-Check: PASSED

- CoffeePeek.AccountService/appsettings.json: FOUND with SendDefaultPii false, MaxRequestBodySize None
- CoffeePeek.ShopsService/appsettings.json: FOUND with SendDefaultPii false, MaxRequestBodySize None
- CoffeePeek.ModerationService/appsettings.json: FOUND with SendDefaultPii false, MaxRequestBodySize None
- Commit 5677280: FOUND in git log
