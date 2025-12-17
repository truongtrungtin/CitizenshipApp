# Citizenship Tutor (Blazor WebAssembly + .NET)

Ứng dụng ôn thi quốc tịch Mỹ cho người lớn tuổi (Civics / Reading / Writing), ưu tiên UI dễ đọc, nút lớn, ít thao tác.

> Admin tạm thời dùng Swagger để tiết kiệm thời gian (không làm admin UI).

## Tech stack (MVP)
- .NET (Api / Worker / Shared / clean layers)
- Blazor WebAssembly (`Ui.Blazor`)
- EF Core (migrations + SQL Server local)
- Auth: JWT (refresh token optional)

## Solution structure
- `Domain/` — Entities + rules (không EF, không HTTP)
- `Application/` — Use cases + interfaces (ports)
- `Infrastructure/` — EF Core + implementations (adapters)
- `Api/` — Web API + DI wiring
- `WorkerService/` — Background jobs (audio generation, scheduled tasks)
- `Ui.Blazor/` — Blazor WebAssembly UI
- `Shared/` — DTOs/contracts dùng chung UI/API

## Local development

### 1) Trust HTTPS dev cert (one-time)
```bash
dotnet dev-certs https --trust
