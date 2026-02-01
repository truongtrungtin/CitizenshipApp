# CitizenshipApp – Summary of Changes

> Date: 2026-02-01
>
> This document summarizes all code + config changes made during this refactor/hardening pass. It intentionally **does not include any real secrets** (passwords, JWT keys, etc.).

## Goals

- Reduce exception/log noise (especially around auth).
- Improve maintainability by removing duplicated logic.
- Improve Ubuntu production readiness (reverse proxy, health checks, docs).
- Keep changes minimal, pragmatic, and test-verified.

## High-level outcomes

- **Security/logging hardening**: removed accidental logging of connection details and improved null/validation paths.
- **JWT consistency**: made user-id claim parsing reliable by adding a standard claim and centralizing parsing.
- **Noise reduction**: controllers now return clean `401` (instead of throwing) when user-id is missing/invalid.
- **Ops readiness**: health endpoints + optional forwarded headers support; Ubuntu + systemd deployment doc; Docker SQL Server compose.

### 2026-01-18 — Step 5: Full settings + onboarding + validation UX

- **Shared contract update**
  - Full settings DTO is `Shared.Contracts.Me.UserSettingContracts`.
  - Uses DataAnnotations for validation.
  - Implemented as a property-based record with **settable properties** (`get; set;`) so Blazor `@bind` works with selects/inputs.

- **API**
  - Added full settings endpoints:
    - `GET /api/me/settings/full`
    - `PUT /api/me/settings/full`
  - Keeps existing MVP endpoint (`/api/me/settings`) for backward compatibility.
  - Fixed the shared primary-key rule when creating missing settings rows:
    - `UserSettings.Id` must equal `UserProfile.Id` (no `Guid.NewGuid()`)

- **UI (Blazor WASM)**
  - Onboarding now collects and saves full settings (Language/FontScale/AudioSpeed/DailyGoalMinutes/Focus/SilentMode), then completes onboarding.
  - Settings page now edits full settings (not only Language + DailyGoalMinutes).
  - UI parses `ProblemDetails`/`ValidationProblemDetails` and shows:
    - General error message
    - Field-level errors close to the relevant controls

---

## API changes (from earlier hardening pass)

### Health checks

- Added `/health/live` and `/health/ready` endpoints.
  - Live = process is up.
  - Ready = includes DB connectivity check.
- Intended usage: load balancer / reverse proxy readiness should hit `/health/ready`.

### Reverse proxy support (Forwarded Headers)

- Added **optional** handling for `X-Forwarded-For` and `X-Forwarded-Proto`.
- Gated behind config (`Proxy:Enabled`) so local/dev behavior stays predictable.

### HTTPS redirection configuration

- Added optional `HttpsRedirection:HttpsPort` support for environments where TLS is terminated at the proxy.

### Auth behavior: reduce exception noise

- Controllers were refactored so auth issues (missing/invalid user id) return a clean `401 Unauthorized` response instead of throwing exceptions.

---

## JWT changes

### Add a standard user-id claim

- Token generation now includes `ClaimTypes.NameIdentifier` in addition to the existing subject (`sub`).
- This makes user-id retrieval consistent across ASP.NET Core conventions and custom code.

---

## Deduplication / Clean-up refactor

### New controller base

- Added `ApiControllerBase` to centralize repeated controller logic:
  - `TryGetUserId(out Guid userId)` pulls user id from claims (`NameIdentifier`, `sub`, fallback `Name`).
  - Controllers can now share the same parsing rules and avoid repeated code.

Files:
- Api/Infrastructure/ApiControllerBase.cs

### Shared question type mapping

- Centralized mapping from raw DB/string values to `QuestionType`.
- This removes duplicated `switch` mapping logic across API/controller/service layers.

Files:
- Shared/Contracts/Deck/QuestionTypeMapper.cs

### Notes on ClaimsPrincipal extensions

- There used to be an attempt to delete a `ClaimsPrincipalExtensions` file; deletion didn’t take effect in this environment.
- The file was replaced with a minimal “intentionally blank” stub to avoid duplicate logic.

Files:
- Api/Infrastructure/ClaimsPrincipalExtensions.cs

---

## Infrastructure / DB configuration hardening

- Removed accidental logging that could disclose SQL connection targets.
- Fixed a null-check ordering bug to validate the connection string before building a SQL connection string builder.
- Improved error messages to point toward environment variables / secret store usage.

(Implemented in Infrastructure dependency injection registration code.)

---

## Worker service changes

- Reduced default heartbeat log spam:
  - Changed loop delay from ~1 second to 1 minute.

Files:
- WorkerService/Worker.cs

- Removed an unnecessary package reference:
  - Dropped `Microsoft.AspNetCore.Components.WebAssembly.DevServer` from the worker project (not used by a Worker Service).

Files:
- WorkerService/WorkerService.csproj

---

## Ubuntu deployment & Docker

### New deployment doc

- Added a full Ubuntu deployment guide:
  - `dotnet publish` output paths
  - example systemd unit files
  - nginx / cloudflared reverse-proxy notes
  - environment variable layout
  - health check endpoints

Files:
- docs/DEPLOY-UBUNTU.md

### SQL Server Docker compose

- Added a sample compose for SQL Server 2022 (Express):
  - persistent volume
  - restart policy
  - password sourced from env file

Files:
- docker-compose.sqlserver.yml

### Example env templates

- Added `.env.example` with safe placeholders.

Files:
- .env.example

IMPORTANT SECURITY NOTE:
- A local `.env` may exist for your server deployment. **Do not commit `.env`** to git.
- If a real SQL password/JWT key was ever shared or committed, rotate it immediately.

---

## Documentation consolidation

- Standardized project documentation under `docs/` as the single source of truth.
- Root-level duplicates (when present) should be replaced by stubs pointing to `docs/*` to prevent drift.

Files:
- docs/*

# Changes Summary

---


## 2026-02-01 — Paging, Worker maintenance, SystemLanguage, tests & CI gate

- **API (Decks)**
  - Added paging for deck questions:
    - `GET /api/decks/{deckId}/questions?page=1&pageSize=50`
  - Validates `page >= 1` and `1 <= pageSize <= 200`, and uses deterministic ordering for stable paging.

- **WorkerService**
  - Added `DbMaintenanceWorker`:
    - Runs once on startup, then daily
    - Executes `SeedData.SeedAsync(db)` and provides a safe place for future retention/cleanup jobs.

- **User settings**
  - Added `SystemLanguage` to `UserSettings` + `UserSettingContracts` and updated UI to edit it.
  - Added EF Core migrations to backfill/default the new column.
  - Defaulted SystemLanguage to Vietnamese and question Language to English.
  - UI language preview + local fallback for SystemLanguage.

- **UI/UX**
  - Responsive navigation (sidebar on desktop, bottom bar on mobile).
  - Home page redesign and bilingual UI labels.
  - Settings/Onboarding labels localized (font sizes, audio speeds, focus).
  - Study UI: localized question type labels and hid dev/admin-only controls.
  - Added Web Speech API TTS with Listen/Stop and per-option audio.

- **Testing**
  - Added unit tests for `JwtTokenService`.
  - Added/expanded integration tests for:
    - Auth flow
    - Study flow
    - Decks/Questions (including paging)

- **CI**
  - Added a formatting gate (`dotnet format --verify-no-changes`) and repo hygiene checks (no tracked `bin/obj`, no real `.env`).

---
## 2026-01-19 — Architecture Hardening & UX Completion

- Standardized API error handling with ProblemDetails
- Added correlation ID middleware
- Added rate limiting to login/register endpoints
- Refactored controllers to Application services
- Optimized study next-question selection
- Implemented full onboarding and settings UX
- Applied global FontScale across UI
- Used Focus setting to suggest default study deck
- Added integration tests for validation error contracts
- Removed legacy MVP `/api/me/settings` endpoint

---

## 2026-01-18 — Full Settings & Onboarding

- Added full settings endpoint `/api/me/settings/full`
- Introduced UserSettingContracts with DataAnnotations
- Added onboarding completion flow
- Improved validation UX across UI
