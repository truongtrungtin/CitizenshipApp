# Project State

Last updated: 2026-01-04

## Current state (as-is)
- Solution builds on .NET 9 (`net9.0`) with Clean Architecture-style project split.
- API has JWT auth + Swagger bearer configured.
- EF Core + SQL Server configured; in Development API auto-migrates + seeds on startup (with retry for DB warm-up).
- UI (Blazor WASM) has pages: register/login/logout, onboarding (mark complete), settings (language + daily goal), study (deck select + answer + progress).
- WorkerService exists but is currently a placeholder (heartbeat loop).

## What's done (implemented in code)
- Auth endpoints: register/login returning JWT.
- User profile + onboarding flag (`UserProfile.IsOnboarded`).
- User settings read/update (`Language`, `DailyGoalMinutes` used by UI).
- Study endpoints: next question, submit answer, today progress.
- SQL schema + migrations + seed sample deck/questions.
- Admin/system hardening:
	- `AppSettingsController` is Admin-only and uses DTOs/contracts (no Domain entity exposure).
- Standardized API errors:
	- Global exception handling returns RFC7807-like `application/problem+json`.
- Question/deck source-of-truth unified:
	- Read-only deck/question queries are DB-backed via EF Core.
	- Embedded JSON question bank is no longer used by DI at runtime.
- Quality gates:
	- CI pipeline runs restore/build/test.
	- API integration tests cover security + decks/questions + study flow (next/answer/today) using an in-memory test host.

## Known issues
- Infrastructure logs: connection target is printed via `Console.WriteLine` in DI (should be moved to ILogger + redacted).
- Architecture consistency: `StudyController` still queries `AppDbContext` directly (not via an Application service).
- Validation: public endpoints do not yet return field-level validation ProblemDetails.
- UX: onboarding route guard and richer onboarding fields are still pending.

## Next milestones
1) Finish controller/service consistency: move remaining direct EF usage (notably study queries) behind Application interfaces.
2) Quality: add request validation + more targeted tests; consider a DB-realistic integration test layer when needed.
3) Observability + DevEx: remove sensitive console output; add health/readiness; add docker-compose for local SQL.
4) MVP UX: route guard for onboarding; extend onboarding to match Domain settings.

## Risk register
- Accidental exposure of system settings via public endpoint.
	- Mitigation: add authz policy/role + stop binding domain entities directly.
- Diverging behavior due to two question-bank sources.
	- Mitigation: pick single source of truth and refactor queries through Application interface.
- Local run friction (SQL Server + secrets).
	- Mitigation: docker compose + validated config + clearer docs.

## Release notes (draft)
- v0.1: Auth + onboarding flag, deck/question seed, study flow + progress, Blazor UI baseline.

