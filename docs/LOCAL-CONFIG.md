# Local Config

Tài liệu này mô tả cách chạy local đúng theo **code hiện tại** (API + UI + DB).

> Source-of-truth: mọi tài liệu dự án nằm trong thư mục `/docs`.

## Ports (theo launchSettings.json)
- API: `https://localhost:7070` (HTTP: `http://localhost:5294`)
- UI:  `https://localhost:7031` (HTTP: `http://localhost:5215`)

## Prerequisites
- .NET SDK 9.x
- SQL Server (Docker hoặc local)

## 1) Trust HTTPS dev cert (one-time)
```bash
dotnet dev-certs https --trust
```

## 2) Database setup

### Option A: SQL Server in Docker (recommended)
Repo đã có file compose: `docker-compose.sqlserver.yml` và `.env.example`.

```bash
cd <repo-root>
cp .env.example .env
# edit .env: set MSSQL_SA_PASSWORD

docker compose -f docker-compose.sqlserver.yml --env-file .env up -d
docker ps
```

Notes:
- DB sẽ lắng nghe ở `localhost:1433`.
- Volume `citizenship_sql_data` giúp DB persist qua restart.

### Option B: SQL Server local instance
Tạo database (ví dụ) `CitizenshipApp` và đảm bảo connection string truy cập được.

## 3) Configure secrets (không commit secrets)

API load secrets từ **user-secrets** trong Development (xem `Api/Program.cs`).

Chạy lệnh tại folder `Api/`:

```bash
cd Api

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=CitizenshipApp;User Id=sa;Password=<YOUR_PASSWORD>;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "<YOUR_LONG_RANDOM_KEY_32+>"

# (Optional) Seed 1 tài khoản admin cho local dev (để gọi endpoint Admin-only)
dotnet user-secrets set "Seed:AdminEmail" "admin@local"
dotnet user-secrets set "Seed:AdminPassword" "ChangeMe123!"
```

Ghi chú:
- `ConnectionStrings:DefaultConnection` là bắt buộc.
- `Jwt:Key` phải >= 32 ký tự (API validate on start).

## 4) Run order (nếu chạy đủ stack)
1) SQL Server (Docker/local)
2) API
   ```bash
   dotnet run --project Api
   ```
3) UI
   ```bash
   dotnet run --project Ui.Blazor
   ```
4) Worker (optional)
   ```bash
   dotnet run --project WorkerService
   ```

## 5) Env vars / .env
- **API/UI không tự động đọc `.env`.**
- `.env` trong repo chủ yếu dùng cho **docker compose** (DB) hoặc cho env-file khi deploy.

Env vars hay dùng:
- API/UI: `ASPNETCORE_ENVIRONMENT=Development`
- Worker: `DOTNET_ENVIRONMENT=Development`

## 6) EF Core migrations (developer notes)
Repo có migrations ở `Infrastructure/Persistence/Migrations`.

Tạo migration mới:
```bash
dotnet ef migrations add <Name> --project Infrastructure --startup-project Api
```

Apply migrations:
```bash
dotnet ef database update --project Infrastructure --startup-project Api
```
