# Citizenship Tutor

Ứng dụng ôn thi quốc tịch Mỹ cho người lớn tuổi (Civics / Reading / Writing), ưu tiên UI dễ đọc, thao tác ít, và tiến trình học đơn giản.

Hiện tại admin tạm dùng Swagger (chưa có admin UI).

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

Mermaid (high-level):

```mermaid
flowchart LR
	UI[Ui.Blazor (WASM)] -->|HTTP JSON + Bearer JWT| API[Api (ASP.NET Core)]
	API --> APP[Application (ports)]
	API --> INFRA[Infrastructure]
	INFRA --> DB[(SQL Server)]
	INFRA --> QB[Embedded QuestionBank JSON (MVP)]
	WORKER[WorkerService] -. background jobs .-> INFRA
```

## User flows (current)
- Register/Login: UI gọi API để nhận JWT và lưu vào local storage.
- Onboarding: UI gọi API để đánh dấu hoàn tất onboarding.
- Study: UI chọn deck, lấy câu hỏi tiếp theo, submit đáp án, xem tiến độ hôm nay.

## Local run

### Prerequisites
- .NET SDK 9.x (repo đang build với `net9.0`)
- SQL Server (local instance hoặc Docker)

### 1) Trust HTTPS dev cert (one-time)
```bash
dotnet dev-certs https --trust
```

### 2) Configure secrets (required)
API đọc cấu hình từ user-secrets ở Development (xem `UserSecretsId` trong `Api/Api.csproj`).

Tại thư mục `Api/`:

```bash
cd Api

# SQL Server connection string (ví dụ; không commit secret thật)
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=CitizenshipApp;User Id=sa;Password=<YOUR_PASSWORD>;TrustServerCertificate=True"

# JWT signing key (>= 32 ký tự)
dotnet user-secrets set "Jwt:Key" "<YOUR_LONG_RANDOM_KEY_32+>"
```

Tuỳ chọn (prod / non-dev):
- `Jwt:Issuer`, `Jwt:Audience`
- `Cors:AllowedOrigins` (môi trường non-development)

### 3) Run database migrations + seed (dev)
Trong Development, API sẽ tự chạy EF migrations + seed lúc startup.
Chỉ cần đảm bảo SQL Server sẵn sàng trước khi chạy API.

### 4) Run API
```bash
dotnet run --project Api
```

- Swagger: `https://localhost:7070/swagger`
- Health ping: `GET https://localhost:7070/api/ping`

Ports theo `Api/Properties/launchSettings.json`:
- HTTPS: `https://localhost:7070`
- HTTP: `http://localhost:5294`

### 5) Run UI
```bash
dotnet run --project Ui.Blazor
```

Ports theo `Ui.Blazor/Properties/launchSettings.json`:
- HTTPS: `https://localhost:7031`
- HTTP: `http://localhost:5215`

UI cấu hình API base URL tại `Ui.Blazor/wwwroot/appsettings.json` (mặc định trỏ về `https://localhost:7070`).

### 6) Run Worker (optional)
```bash
dotnet run --project WorkerService
```

## Configuration

### API configuration keys
- `ConnectionStrings:DefaultConnection` (required)
- `Jwt:Key` (required, >= 32 ký tự)
- `Jwt:Issuer` (default: `CitizenshipApp`)
- `Jwt:Audience` (default: `CitizenshipApp.Ui`)
- `Jwt:AccessTokenMinutes` (default: 60)
- `Cors:AllowedOrigins` (non-development)

### UI configuration keys
- `Api:BaseUrl` (default: `https://localhost:7070`)

## Common commands
```bash
dotnet build CitizenshipApp.sln
dotnet test CitizenshipApp.sln

# EF migrations (run from Api project so design-time services resolve)
dotnet ef migrations add <Name> --project Infrastructure --startup-project Api
dotnet ef database update --project Infrastructure --startup-project Api
```

## Troubleshooting
- "Missing ConnectionStrings:DefaultConnection": set user-secrets for Api (see Local run step 2).
- SQL Server in Docker not ready: API retries migrations in Development; wait a few seconds and retry.
- CORS errors: in Development CORS allows any origin; in non-dev set `Cors:AllowedOrigins`.

## Contributing conventions
- Keep Domain free of EF/HTTP dependencies.
- API routes: prefix `api/` and use noun-based resources.
- Prefer DTOs in `Shared/Contracts` for public API payloads.

Branch/commit (suggested):
- Branch: `feature/<short-name>` or `fix/<short-name>`
- Commit: Conventional Commits (`feat:`, `fix:`, `docs:`, `refactor:`)

## License / notes
Internal project (no explicit license file found in repo).
