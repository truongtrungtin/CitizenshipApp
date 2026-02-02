# CitizenshipApp – Technical Blueprint (Developer-Follow)

> **Note:** The PDF blueprint `docs/CitizenshipApp_Technical_Blueprint.pdf` is a snapshot around **2026-01-19**.  
> This Markdown file is the **living blueprint** updated to match the current repo state (**2026-02-01**).

---

## 1) Product goal

App ôn thi quốc tịch Mỹ cho người lớn tuổi (elderly-first): thao tác ít, chữ lớn, nút lớn, phản hồi rõ ràng, lỗi dễ hiểu.

Non-functional requirements:
- API contracts shared with UI (Shared/Contracts)
- RFC7807 ProblemDetails + field-level ValidationProblemDetails
- JWT auth + rate limiting for public auth endpoints
- Correlation Id per request/response

---

## 2) Repo structure & responsibilities

Solution: `CitizenshipApp.sln`

Projects:
- `Api/` — ASP.NET Core Web API (composition root: DI + middleware + controllers)
- `Application/` — interfaces/use-case contracts (ports) so Api doesn’t depend on EF
- `Domain/` — pure entities/enums (no EF/ASP.NET dependencies)
- `Infrastructure/` — EF Core + Identity + seed data + Application implementations
- `Shared/` — public Contracts used by API + UI + Worker
- `Ui.Blazor/` — Blazor WebAssembly frontend
- `WorkerService/` — background maintenance service
- `Tests/` — unit + integration tests

Docs single source of truth: `docs/*`

---

## 3) Clean Architecture rules

Dependency direction:
- Domain → (no dependencies)
- Application → Domain + Shared
- Infrastructure → Application + Domain + Shared + EF/Identity
- Api → Application + Infrastructure + Shared (composition only)
- Ui.Blazor → Shared (contracts) + Api endpoints

Controllers are thin:
- input binding + ModelState validation
- call Application service interface
- return Shared contracts

---

## 4) Local run

See: `docs/LOCAL-CONFIG.md`

Recommended DB: SQL Server via Docker:
- `docker-compose.sqlserver.yml` + `.env.example`

Run order:
1) DB via docker compose
2) API: `dotnet run --project Api`
3) UI: `dotnet run --project Ui.Blazor`
4) Worker (optional): `dotnet run --project WorkerService`

---

## 5) API behaviors

Cross-cutting (in `Api/Program.cs`):
- Global exception → ProblemDetails
- ModelState invalid → ValidationProblemDetails
- Rate limiting for auth endpoints
- CorrelationId middleware (`X-Correlation-ID`)
- Health endpoints: `/health/live`, `/health/ready`

Key endpoints (MVP):
- Auth: register/login (JWT)
- Me: profile + settings (full)
- Decks:
  - `GET /api/decks`
  - `GET /api/decks/{deckId}`
  - `GET /api/decks/{deckId}/questions?page=1&pageSize=50` (bounded; stable paging)
- Study: next-question + answer flow

Paging rule:
- `page` is 1-based
- `pageSize` in `[1..200]`
- Stable ordering before Skip/Take

---

## 6) Settings model (elderly-first)

Two language settings (explicit split):
- `SystemLanguage` = UI/system text language (default `Vi`)
- `Language` = question content language (default `En`)

Other settings:
- FontScale, AudioSpeed, DailyGoalMinutes, Focus, SilentMode

---

## 7) WorkerService

`DbMaintenanceWorker` runs:
- once on startup
- then every 24 hours

Currently it:
- executes `SeedData.SeedAsync(db)`
- provides a safe place for future retention/cleanup jobs

---

## 8) Testing & CI

Tests:
- Unit: `JwtTokenServiceTests`
- Integration: Auth flow, Study flow, Decks/Questions (paging), Settings validation contracts

CI (`.github/workflows/ci.yml`):
- repo hygiene checks (no tracked `bin/obj`, no real `.env`)
- `dotnet format --verify-no-changes`
- build + test on ubuntu-latest

---

## 9) Remaining roadmap (high-level)

Open item:
- (none)

Completed:
- BL-029: Audio/TTS integration (implemented in UI with Web Speech API + voice setting)
