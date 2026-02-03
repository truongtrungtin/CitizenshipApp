---

# 2️⃣ `docs/PROJECT-STATE.md` (FULL FILE)

# Project State

Last updated: 2026-02-03

---

## Current State

CitizenshipApp is a production-ready .NET 9 application with a focus on
accessibility, clean architecture, and predictable behavior.

---

## API

- JWT authentication with consistent userId extraction
- Global exception handling using RFC-compliant ProblemDetails
- Validation errors returned as ValidationProblemDetails (field-level)
- CorrelationId middleware:
  - Accepts incoming correlation ID or generates one
  - Attaches correlationId to all ProblemDetails
- Rate limiting applied to public auth endpoints
- Health endpoints:
  - `/health/live`
  - `/health/ready`
- Production CORS is deny-by-default unless explicitly configured
- Deck browsing endpoints support paging for large question lists:
  - `GET /api/decks/{deckId}/questions?page=1&pageSize=50`
  - `page >= 1`, `1 <= pageSize <= 200`

---

## UI (Blazor WASM)

### Pages
- Home
- Register
- Login
- Logout
- Onboarding (mandatory)
- Settings (full)
- Study

### UX Rules
- Users can access Home without login; onboarding is required before Study/Settings
- FontScale applies globally via CSS variables
- Focus is used to suggest a default study deck
- Language model:
  - `Language` = question content language
  - `SystemLanguage` = UI/system text language
- UI language uses a local fallback (persisted) until full settings load
- All forms display clear field-level validation errors
- Study supports TTS via Web Speech API (Listen/Stop + option audio)
- TTS voice preference is stored in settings and applies to speech output

---

## Architecture

- Controllers are thin
- Business logic lives in Application services
- Study logic optimized for large datasets
- EF Core migrations manage schema evolution
- SQL Server via Docker for local/dev
- WorkerService includes a maintenance job:
  - Runs at startup + daily
  - Executes SeedData and provides hooks for future cleanup/retention jobs

---

## Testing

- CI pipeline builds and runs tests
- CI includes a formatting gate:
  - `dotnet format --verify-no-changes`
  - Fails if tracked build artifacts (`bin/`, `obj/`) or real `.env` files are committed
- Unit tests validate:
  - JWT token generation/claims/validation
- Integration tests validate:
  - Auth flow (register/login + authorized requests)
  - Study flow (next-question and answer progression)
  - Deck/Question endpoints (including paging)
  - ValidationProblemDetails contracts (auth + full settings)

---

## Completed Backlog

BL-001 → BL-026
BL-027
BL-028
BL-029
BL-030
BL-031
BL-032
BL-033
BL-035
BL-036

---

## Open / Future Items

### P1 (near-term)
- BL-037: Admin import questions (CSV/JSON)
- BL-038: Admin edit question/answer texts (Swagger)
- BL-039: Study sessions domain + start session API
- BL-040: SRS scheduler + next/grade endpoints
- BL-041: Blazor Learn session UI (session-based)
- BL-042: Quiz MCQ generator + frozen choices per item
- BL-043: Silent lock MCQ-only flow (API + UI)

### P2 (mid-term)
- BL-044: Audio cache file-based (hash + storage + serving)
- BL-045: Blazor audio playback controls (cached audio URLs)
- BL-046: Integration tests for sessions/quiz/silent/SRS
- BL-047: Self-host deploy checklist + backup DB/audio

### P3 (later)
- BL-048: AI generation pipeline (admin + worker + approve)
- BL-049: Dynamic facts (schema + refresh job)

### P4 (mobile readiness)
- BL-050: MAUI Blazor Hybrid stub
- BL-051: Mobile responsive UI improvements
- BL-052: Mobile offline study mode (local storage sync)
- BL-053: Mobile TTS offline support (platform APIs)
- BL-054: Mobile push notifications (reminders)
- BL-055: Mobile device settings sync (language/voice)
- BL-056: Mobile E2E tests (device/emulator)
```
