# CitizenshipApp – Summary of Changes

> Date: 2026-01-14
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
- **Maintainability**: added shared helpers to eliminate duplicated logic (controller base + question type mapping).

---

## API changes

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

## Build artifacts & repo hygiene (observed)

The git working tree currently shows changes in build output folders (for example `obj/` under Blazor/Worker projects). Those are **build artifacts** and should not be tracked in source control.

Recommended follow-ups:
- Ensure `bin/` and `obj/` are in `.gitignore`.
- If `obj/` is already tracked, remove it from git index going forward (e.g., `git rm -r --cached **/obj **/bin`) and keep the directories locally.

Also observed:
- A stray editor swap file like `..env.swp` may appear; it should not be committed.

---

## Tests / verification

- Verified with:

```bash
dotnet test CitizenshipApp.sln -c Release
```

- Result: tests pass (10 passed, 0 failed at last run).

---

## Files touched (high-signal)

New:
- Api/Infrastructure/ApiControllerBase.cs
- Api/Infrastructure/ClaimsPrincipalExtensions.cs (intentionally blank stub)
- Shared/Contracts/Deck/QuestionTypeMapper.cs
- docs/DEPLOY-UBUNTU.md
- docker-compose.sqlserver.yml
- .env.example

Updated:
- WorkerService/Worker.cs
- WorkerService/WorkerService.csproj

Other changes may also exist from earlier hardening work (API pipeline, JWT service, DI, controllers, appsettings). Those were part of the same “read + harden + dedupe + deploy” pass.
