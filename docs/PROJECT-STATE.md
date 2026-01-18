# Project State

Last updated: 2026-01-18

## Current state (as-is)
- Solution builds on **.NET 9** (`net9.0`) with a Clean Architecture-style split.
- **API**
  - JWT Bearer auth (Swagger includes Bearer support).
  - Global exception handler returns **ProblemDetails** (`application/problem+json`).
  - **Model Validation**: DataAnnotations + automatic `ValidationProblemDetails` for invalid payloads (field-level errors).
  - **Health** endpoints:
    - `GET /health/live` (process up)
    - `GET /health/ready` (includes DB connectivity)
  - **CORS**
    - Development: allows any origin/header/method.
    - Non-development: uses `Cors:AllowedOrigins`; if not configured, **cross-origin requests are denied** (safe default).
  - **Reverse proxy** support is **disabled by default**; enable via `Proxy:Enabled=true` to respect `X-Forwarded-*` headers.
  - Development convenience:
    - Auto-migrate + seed on startup with retry for SQL container warm-up.

- **UI (Blazor WASM)** pages:
  - Register/Login/Logout
  - **Onboarding (required)**: collects and saves full settings, then marks onboarding complete
  - **Settings (full)**: Language, FontScale, AudioSpeed, DailyGoalMinutes, Focus, SilentMode
  - Study (deck select → next question → submit answer → today progress)

- **Route guard**
  - Not authenticated → only `/login` + `/register`
  - Authenticated but not onboarded → forced to `/onboarding`
  - Authenticated & onboarded → redirected away from `/login` + `/register`

- **WorkerService** exists but currently a placeholder loop (no meaningful background jobs yet).

## What's done (implemented in code)
- Auth:
  - Register/Login endpoints returning JWT.
  - JWT contains both `sub` and `ClaimTypes.NameIdentifier` to keep userId parsing consistent across controllers.
- Profile & onboarding:
  - `UserProfile.IsOnboarded` stored and updated via API.
  - UI enforces onboarding via route guard.
- Settings:
  - Full settings contract: `Shared.Contracts.Me.UserSettingContracts`.
    - Uses DataAnnotations.
    - Uses **settable properties** to support Blazor `@bind`.
  - API supports full settings read/update via `GET/PUT /api/me/settings/full`.
  - UI Settings + Onboarding read/write full settings.
- Validation + UX errors:
  - API returns validation errors as `ValidationProblemDetails`.
  - UI parses ProblemDetails and shows:
    - Top-level message
    - Field-level errors next to controls (Login/Register/Settings/Study/Onboarding)
- Study:
  - Endpoints for next question, submit answer, and today progress aggregation.
- Persistence:
  - EF Core migrations + SQL Server schema.
  - Seed sample deck/questions.

## Known issues / gaps
- UI preference effects are not fully applied yet:
  - `FontScale` is stored but not yet applied to global UI typography.
  - `AudioSpeed` / `SilentMode` are stored but not yet wired to a TTS/audio feature.
  - `Focus` is stored but not yet used to filter/suggest decks.
- Architecture consistency:
  - `StudyController` still queries `AppDbContext` directly (not via an Application service).
- Performance:
  - Next-question selection still uses simple selection patterns (may need improvement at large scale).
- Worker:
  - WorkerService is not yet doing real jobs.

## Next milestones (recommended order)
1) **Apply settings to UI behavior**
   - FontScale → global CSS scaling (M/L/XL)
   - Focus → default deck selection/suggestions
   - AudioSpeed/SilentMode → when TTS is implemented
2) **Controller/service consistency**
   - Continue BL-017: move study/deck queries behind Application services.
3) **API performance**
   - Improve next-question selection strategy for large datasets.
4) **Observability & hardening**
   - Request logging + correlation id
   - Rate-limit auth endpoints
5) **Worker skeleton**
   - Replace placeholder loop with a real background job framework.

## Risk register
- Storing UI preferences but not applying them can confuse users.
  - Mitigation: implement FontScale first (visible impact).
- Direct EF usage in controllers may cause duplicated logic and makes future optimization harder.
  - Mitigation: continue refactor via Application interfaces.

## Release notes (draft)
- v0.2: Onboarding route guard + full settings (store + edit) + field-level validation errors in UI.
