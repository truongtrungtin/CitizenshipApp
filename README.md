# CitizenshipApp

CitizenshipApp is a .NET 9 application designed to help adult and elderly learners
prepare for the U.S. Citizenship test through guided study, onboarding, and
accessible UI/UX.

The project emphasizes:
- Clean architecture
- Predictable API contracts
- Strong validation and error UX
- Accessibility (large fonts, minimal steps)
- Scalability for future learning features

---

## Architecture Overview

The solution follows a layered architecture:

- **Api**
  ASP.NET Core Web API (JWT auth, ProblemDetails, rate limiting)

- **Application**
  Business logic, services, and use-cases

- **Infrastructure**
  EF Core, SQL Server, persistence, migrations

- **Shared**
  Contracts/DTOs shared between API and UI

- **Ui.Blazor**
  Blazor WebAssembly frontend

- **WorkerService**
  Background processing (currently skeleton)

- **Tests**
  Unit and integration tests

---

## Key Features

### Authentication & Security
- JWT authentication
- Standardized ProblemDetails error responses
- Field-level validation errors
- Rate limiting on login/register endpoints
- Correlation ID for request tracing

### User Experience
- Mandatory onboarding after registration
- Full user settings:
  - Language
  - FontScale
  - AudioSpeed
  - DailyGoalMinutes
  - Focus (Civics / Reading / Writing)
  - SilentMode
- Global FontScale applied across entire UI
- Study flow suggests default deck based on Focus
- Clear error feedback on all forms

### Study Flow
- Deck-based learning
- Optimized next-question selection (no Count+Skip)
- Daily progress tracking

---

## API Endpoints (Current)

### Authentication
- `POST /api/auth/register`
- `POST /api/auth/login`

### User Settings
- `GET /api/me/settings/full`
- `PUT /api/me/settings/full`
- `PUT /api/me/onboarding/complete`

> ⚠️ Legacy MVP settings endpoint has been **removed**.
> `/api/me/settings/full` is the **single source of truth**.

### Health
- `GET /health/live`
- `GET /health/ready`

---

## Documentation

All project documentation lives in `/docs`:

- [`docs/PROJECT-STATE.md`](docs/PROJECT-STATE.md)
- [`docs/CHANGES-SUMMARY.md`](docs/CHANGES-SUMMARY.md)
- [`docs/DECISIONS.md`](docs/DECISIONS.md)
- [`docs/BACKLOG.csv`](docs/BACKLOG.csv)
- [`docs/UX-RULES.md`](docs/UX-RULES.md)
- [`docs/LOCAL-CONFIG.md`](docs/LOCAL-CONFIG.md)

---

## Development

### Prerequisites
- .NET SDK 9.x
- Docker (for SQL Server)

### Run locally
```bash
docker compose up -d
dotnet run --project Api
dotnet run --project Ui.Blazor
