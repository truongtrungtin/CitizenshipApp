# Work Status

Last updated: 2026-02-03

## ✅ Done (high-level summary)
- Architecture baseline: clean layering, Identity in Infrastructure, JWT auth, ProblemDetails, CORS, rate limiting.
- Core UX: onboarding guard, settings, font scale, focus-based deck suggestion, TTS with voice preference.
- Tests: unit + integration + Playwright E2E (seed endpoint, CI job).
- CI: formatting gate, repo hygiene, E2E stability hardening.

## ✅ Done (detailed)
Source of truth for status: [docs/BACKLOG.csv](docs/BACKLOG.csv)

## Assumptions / conventions
- All API routes are under `/api`.
- Backlog status and priorities follow [docs/BACKLOG.csv](docs/BACKLOG.csv).

### Platform & architecture
- BL-001 → BL-006: secured AppSettings, admin role baseline, standardized errors, unified deck/question source, fixed userId claims, request validation.
- BL-010 → BL-018: removed secret logging, nullability fixes, correlation IDs, rate limiting, production CORS, controller/service refactor, study optimization.
- BL-019: paging for deck questions (stable ordering + validation).

### Data & infrastructure
- BL-012: SQL Server Docker setup for local dev.
- BL-013: health endpoints (/health/live, /health/ready).
- BL-020: WorkerService maintenance job.
- BL-024/025: CI build pipeline + format gate.
- BL-034: Voice column hotfix migration + encoding cleanup.
- BL-026: docs consolidation (docs/ as single source of truth).

### UI/UX & settings
- BL-007/008/009: improved UI error handling, onboarding guard, full settings + onboarding domain.
- BL-027/028: global FontScale + focus-based deck suggestion.
- BL-029/033: TTS audio + voice preference.
- BL-031/032: legacy MVP settings removal + SystemLanguage support.

### Testing
- BL-021/022/023/030: JWT unit tests + auth/study/validation integration tests.
- BL-035: Playwright E2E suite + seed endpoint + CI job.
- BL-036: E2E stability hardening (HTTP-only CI readiness, HTTPS redirect guard, diagnostics).

## 🟡 In progress
- None

## ⏳ Next (planned)
The items below are derived from the backlog and grouped by priority.

## 🔧 Tech & libraries to support upcoming work
This section lists the expected technologies/packages by area. Final choices may change as implementation begins.

### API/Domain/Infrastructure
- ASP.NET Core Controllers (existing)
- EF Core migrations + SQL Server
- Swagger/OpenAPI for admin endpoints
- CSV/JSON import:
	- CSV: `CsvHelper` (recommended)
	- JSON: `System.Text.Json` (existing)
- Background jobs:
	- HostedService (existing WorkerService)
	- Optional: `Quartz.NET` if scheduling grows complex
- File storage for audio cache:
	- Local filesystem + static file middleware
	- Hashing: `System.Security.Cryptography` (SHA256)
- Validation:
	- DataAnnotations (existing)
	- Optional: `FluentValidation` if rules become complex

### UI (Blazor WebAssembly)
- Blazor components + HttpClient (existing)
- Audio playback:
	- HTML5 Audio (`Audio` element) + JS interop
- File upload (optional / future if we ever add admin UI; for now admin is Swagger-only):
	- `InputFile` component

### Testing
- NUnit (existing)
- Playwright (existing)
- Integration tests:
	- `WebApplicationFactory` (existing)
- Optional for complex schedulers:
	- Time abstraction: `IClock` (recommended); optional: `NodaTime` later

### Mobile readiness
- .NET MAUI Blazor Hybrid (future stub)
- Notifications:
	- MAUI Essentials notifications or platform-specific services
- Offline storage:
	- `IndexedDB` (web) or local SQLite (MAUI)

### P1 (near-term)
Sequence: Admin import/edit → sessions/SRS → MCQ generation → silent lock.

#### BL-037 — Admin import questions (CSV/JSON)
- Endpoint: `POST /api/admin/import/questions`
- Upsert rules: `(TestVersion, QuestionNo)` as natural key
- Actions: create/update question + answers + texts
- Output: summary (created/updated/skipped/errors)

#### BL-038 — Admin edit question/answer texts (Swagger)
- Endpoints:
	- `PUT /api/admin/questions/{id}/texts`
	- `PUT /api/admin/answers/{id}/texts`
- Optional audit fields: `UpdatedBy`, `UpdatedAt`

#### BL-039 — Study sessions domain + start session API
- New entities: `StudySession`, `StudySessionItem`
- API: `POST /api/study/sessions/start` (mode, deckId, ruleId)
- Returns: `sessionId` + initial state

#### BL-040 — SRS scheduler + next/grade endpoints
- API:
	- `GET /api/study/sessions/{id}/next`
	- `POST /api/study/sessions/items/{itemId}/grade` (again/good)
- Logic: due items first, then mix new
- Update `UserQuestionState` via scheduler rules

#### BL-041 — Blazor Learn session UI (session-based)
- UI driven by `sessionId` + `itemId`
- Flow: question → reveal → self-grade → next
- Respect settings (shuffle/explain/phonetic)

#### BL-042 — Quiz MCQ generator + frozen choices per item
- Generate 4 choices (1 correct + 3 distractors)
- Persist choices per `StudySessionItem` to keep stable across retries

#### BL-043 — Silent lock MCQ-only flow (API + UI)
- API: `POST /api/study/sessions/{id}/silent-on`
- Effects: `SilentMode=1`, `ForceMcqOnly=1`
- UI: “Tạm thời tôi không thể nói hoặc nghe” toggle line

### P2 (mid-term)
Sequence: Audio cache → UI playback → integration tests → deploy/backups.
#### BL-044 — Audio cache (file-based)
- Hash: `text+voice+rate+lang` → `AudioAsset`
- Store: `data/audio/{hash}.mp3`
- Serve: immutable cache headers (+ optional range support)
- Cleanup strategy (optional)

#### BL-045 — Blazor audio playback controls
- UI: playback buttons for question/answer in Learn mode
- Uses cached audio URLs
- Respects AudioSpeed/Voice settings

#### BL-046 — Integration tests for sessions/quiz/silent/SRS
- Tests: start session, due-vs-new, grade updates, MCQ stability, silent lock

#### BL-047 — Self-host deploy checklist + backups
- Docs: migrations, run API/UI/Worker, reverse proxy HTTPS
- Backups for SQL + `data/audio`
- Restore + smoke test steps

### P3 (later)
Sequence: AI generation → dynamic facts.
#### BL-048 — AI generation pipeline
- Tables: `AiGenerationJob`, `AiGenerationItem`
- Admin endpoints: create/list
- Worker runner with retries
- Approve/edit flow (Swagger)

#### BL-049 — Dynamic facts
- Schema: `DynamicFact` + bindings
- Worker refresh job
- API resolves with user `StateCode`
- Safe fallback on refresh failure

### P4 (mobile readiness)
Sequence: MAUI stub → responsive UI → offline + TTS → notifications/sync → E2E.
#### BL-050 — MAUI Blazor Hybrid stub
- Project stub hosting Blazor
- Reuse Shared contracts/client

#### BL-051 — Mobile responsive UI
- Layout/navigation improvements for small screens
- Touch target sizing + responsive grids

#### BL-052 — Mobile offline study mode
- Cache decks/questions locally
- Sync answers when online
- Handle conflicts

#### BL-053 — Mobile TTS offline support
- Use platform TTS APIs offline
- Fallback to Web Speech API online

#### BL-054 — Mobile push notifications
- Local notifications for reminders
- Opt-in/out in settings

#### BL-055 — Mobile device settings sync
- Sync SystemLanguage + Voice with device preferences

#### BL-056 — Mobile E2E tests
- Device/emulator E2E flows
- Integrate into CI

## Changes since last update
- Synced with expanded backlog (BL-037 → BL-056) and detailed planned work.
- Added tech/library support notes and clarified API route conventions.

## Risks / blockers
- None reported.

## Next checkpoint
- 2026-02-10
- Goal: complete BL-037 + BL-038 (admin import/edit)
