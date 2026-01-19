---

# 2️⃣ `docs/PROJECT-STATE.md` (FULL FILE)

```md
# Project State

Last updated: 2026-01-19

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

---

## UI (Blazor WASM)

### Pages
- Register
- Login
- Logout
- Onboarding (mandatory)
- Settings (full)
- Study

### UX Rules
- Users must complete onboarding before accessing the app
- FontScale applies globally via CSS variables
- Focus is used to suggest a default study deck
- All forms display clear field-level validation errors

---

## Architecture

- Controllers are thin
- Business logic lives in Application services
- Study logic optimized for large datasets
- EF Core migrations manage schema evolution
- SQL Server via Docker for local/dev

---

## Testing

- CI pipeline builds and runs tests
- Integration tests validate:
  - ValidationProblemDetails for auth
  - ValidationProblemDetails for full settings

---

## Completed Backlog

BL-001 → BL-018
BL-024
BL-027
BL-028
BL-030
BL-031

---

## Open / Future Items

- BL-019: Paging for large question lists
- BL-020: WorkerService background jobs
- BL-021: JWT unit tests
- BL-022: Auth flow integration tests
- BL-023: Study flow integration tests
- BL-025: CI analyzers / formatting gate
- BL-029: Audio/TTS integration
