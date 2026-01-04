# Project State

Last updated: 2026-01-03

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

## Known issues
- Security: `AppSettingsController` exposes unauthenticated CRUD for `AppSettings`.
- Data source split: question bank exists as embedded JSON (Infrastructure) but UI flows use SQL-backed decks/questions; endpoints are inconsistent.
- Error handling is not standardized (string messages, no global ProblemDetails), UI surfaces raw exceptions.
- Minor: build warning in StudyController mapping null `PromptVi` into non-null contract field.

## Next milestones
1) Stabilization: secure admin/system endpoints, unify data source, add consistent error handling.
2) Quality: tests (API + integration), CI pipeline, basic observability.
3) MVP UX: route guard for onboarding, refine onboarding fields to match Domain settings.

## Risk register
- Accidental exposure of system settings via public endpoint.
	- Mitigation: add authz policy/role + stop binding domain entities directly.
- Diverging behavior due to two question-bank sources.
	- Mitigation: pick single source of truth and refactor queries through Application interface.
- Local run friction (SQL Server + secrets).
	- Mitigation: docker compose + validated config + clearer docs.

## Release notes (draft)
- v0.1: Auth + onboarding flag, deck/question seed, study flow + progress, Blazor UI baseline.

