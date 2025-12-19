# Citizenship Tutor — Project State

Last updated: **2025-12-18**

## Current state
- We are starting **Epic 2 (MVP Web + API baseline)**.
- Backend: .NET Web API + EF Core + SQL Server; Clean Architecture layers exist (Domain/Application/Infrastructure/Api).
- Swagger is wired up with an API-key header (`X-Admin-Key`) for testing protected `/api/*` routes in dev.
- EF Core is configured and migrations can be applied on startup in Development.

## DONE
- **2001** Create solution structure (Domain/Application/Infrastructure/Api).
- **2002** Add Clean Architecture references & base DI wiring.
- **2003** Add `.editorconfig` and formatting conventions.
- **2004** Add README and basic run instructions.
- **2007** Create initial migrations and update local SQL Server database (EF Core migrations + `Database.Migrate()` on startup in Development).
- **1001** EPIC 1 — Bootstrap project + Clean Architecture skeleton (Epic marked complete; all child tasks are DONE).

## CHANGED files/paths
- `CitizenshipApp/Api/Program.cs`
- `CitizenshipApp/Api/Api.csproj`
- `CitizenshipApp/Api/appsettings.Development.json`

## Decisions
- 2025-12-18 — Upgraded Swagger setup to Swashbuckle.AspNetCore v10.x and updated OpenAPI types/usings accordingly (uses `Microsoft.OpenApi` namespace + v10 `AddSecurityRequirement(Func<OpenApiDocument,...>)`).
- 2025-12-18 — Added temporary API key gate for `/api/*` routes via `X-Admin-Key` middleware; Swagger is configured to include the header.

## NEXT
- **2008** Add onboarding flag `IsOnboarded` to `UserProfile` — needed to support first-run onboarding UX and gating.
- **2009** Seed minimal system data — needed for consistent local dev/test environment.
- **2010** Configure JWT auth + protect `/admin` endpoints + Swagger bearer — replace temporary `X-Admin-Key` gate with proper auth/roles.
