# Citizenship Tutor

Ứng dụng ôn thi quốc tịch Mỹ cho người lớn tuổi (Civics / Reading / Writing), ưu tiên UI dễ đọc, thao tác ít, và tiến trình học đơn giản.

## Mục tiêu MVP
- Đăng ký / đăng nhập
- **Onboarding bắt buộc** (cài đặt ban đầu) và chặn route cho tới khi onboard xong
- Chọn bộ câu hỏi (deck) → làm câu hỏi → chấm đúng/sai → xem tiến độ trong ngày
- **Cấu hình (Settings) đầy đủ**:
  - Language
  - FontScale (M/L/XL)
  - AudioSpeed (Slow/Normal)
  - DailyGoalMinutes
  - Focus (Civics/Reading/Writing)
  - SilentMode

## Tech stack
- .NET 9 (Web API, Worker, shared contracts)
- Blazor WebAssembly (UI)
- EF Core 9 + SQL Server (Identity + domain tables)
- Auth: JWT Bearer (access token)

## Architecture overview
Solution theo hướng Clean Architecture:
- Domain: entities/enums thuần
- Application: interfaces/use-cases (ports)
- Infrastructure: EF Core + Identity storage + adapters
- Api: ASP.NET Core Web API (composition root)
- Ui.Blazor: Blazor WASM (client)
- Shared: DTO/contracts dùng chung giữa API và UI

High-level diagram:

```mermaid
flowchart LR
  UI[Ui.Blazor (WASM)] -->|HTTP JSON + Bearer JWT| API[Api (ASP.NET Core)]
  API --> APP[Application (ports)]
  API --> INFRA[Infrastructure]
  INFRA --> DB[(SQL Server)]
  WORKER[WorkerService] -. background jobs .-> INFRA
```

## Runtime behavior (code hiện tại)

### Security
- Swagger hỗ trợ Bearer JWT.
- `AppSettingsController` là **Admin-only**.
- JWT tạo ra gồm:
  - `sub`
  - `ClaimTypes.NameIdentifier` (để parse userId ổn định)
  - role claims (`ClaimTypes.Role`)

### Validation
- Request payloads dùng DataAnnotations.
- Invalid payload trả về `ValidationProblemDetails` (field-level errors) với content-type `application/problem+json`.

### CORS
- Development: AllowAnyOrigin/AnyHeader/AnyMethod.
- Non-development:
  - đọc `Cors:AllowedOrigins`
  - nếu không cấu hình origins, API **deny cross-origin** (safe default).

### Reverse proxy
- Forwarded headers (X-Forwarded-For/Proto) là **opt-in** qua `Proxy:Enabled=true`.

### Health
- `GET /health/live`: process up.
- `GET /health/ready`: includes DB check.

### Settings endpoints
- MVP (2 fields): `GET/PUT /api/me/settings`
- Full settings: `GET/PUT /api/me/settings/full`

## Local run
Chi tiết xem: [LOCAL-CONFIG.md](LOCAL-CONFIG.md)

Tóm tắt nhanh:
1) Start SQL Server (khuyến nghị docker compose: `docker-compose.sqlserver.yml`).
2) Set user-secrets cho Api:
   - `ConnectionStrings:DefaultConnection`
   - `Jwt:Key` (>= 32 ký tự)
3) Run API:
   ```bash
   dotnet run --project Api
   ```
4) Run UI:
   ```bash
   dotnet run --project Ui.Blazor
   ```

## Deploy (Ubuntu)
Xem: [DEPLOY-UBUNTU.md](DEPLOY-UBUNTU.md)

## Project docs index
- [PROJECT-STATE.md](PROJECT-STATE.md)
- [DECISIONS.md](DECISIONS.md)
- [BACKLOG.csv](BACKLOG.csv)
- [CHANGES-SUMMARY.md](CHANGES-SUMMARY.md)
- [UX-RULES.md](UX-RULES.md)
- [WIREFRAMES.md](WIREFRAMES.md)
