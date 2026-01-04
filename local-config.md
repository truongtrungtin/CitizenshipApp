# Local Config

Tài liệu này mô tả cách chạy local đúng theo code hiện tại (API + UI + DB).

## Ports (theo launchSettings.json)
- API: `https://localhost:7070` (http: `http://localhost:5294`)
- UI: `https://localhost:7031` (http: `http://localhost:5215`)

## Prerequisites
- .NET SDK 9.x
- SQL Server (local hoặc Docker)

## 1) HTTPS dev cert (one-time)
```bash
dotnet dev-certs https --trust
```

## 2) Database setup

### Option A: SQL Server in Docker (recommended)
Ví dụ chạy SQL Server 2022:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=<YOUR_PASSWORD>" \
	-p 1433:1433 --name citizenship-sql -d mcr.microsoft.com/mssql/server:2022-latest
```

### Option B: SQL Server local instance
Tạo database (ví dụ) `CitizenshipApp` và đảm bảo connection string truy cập được.

## 3) Configure secrets (không commit secrets)

API load secrets từ user-secrets trong Development (xem `Api/Program.cs`).

Chạy lệnh tại folder `Api/`:

```bash
cd Api

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=CitizenshipApp;User Id=sa;Password=<YOUR_PASSWORD>;TrustServerCertificate=True"
dotnet user-secrets set "Jwt:Key" "<YOUR_LONG_RANDOM_KEY_32+>"
```

Ghi chú:
- `ConnectionStrings:DefaultConnection` là bắt buộc (Infrastructure sẽ throw nếu thiếu).
- `Jwt:Key` phải >= 32 ký tự (API validate on start).

## 4) Run order (nếu chạy đủ stack)
1) SQL Server (Docker/local)
2) API (`dotnet run --project Api`)
3) UI (`dotnet run --project Ui.Blazor`)
4) Worker (`dotnet run --project WorkerService`) — optional

## 5) Env vars
Project hiện không dùng `.env` file.

Các env vars liên quan:
- API/UI: `ASPNETCORE_ENVIRONMENT=Development`
- Worker: `DOTNET_ENVIRONMENT=Development`

### Optional `.env.example`
Nếu bạn muốn chuẩn hoá theo `.env` về sau, đây là ví dụ (không được commit secret thật):

```env
# API
ConnectionStrings__DefaultConnection=Server=localhost,1433;Database=CitizenshipApp;User Id=sa;Password=<YOUR_PASSWORD>;TrustServerCertificate=True
Jwt__Key=<YOUR_LONG_RANDOM_KEY_32+>
Jwt__Issuer=CitizenshipApp
Jwt__Audience=CitizenshipApp.Ui

# UI
Api__BaseUrl=https://localhost:7070
```

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
