# DECISIONS (Architecture Decision Records)

Project: Citizenship Tutor
Last updated: 2026-01-03

Format cho mỗi ADR:
- Context
- Decision
- Consequences
- Status
- Date

---

## ADR-001 — Clean Architecture layering
Context:
- Solution chia thành Domain / Application / Infrastructure / Api / Ui.Blazor / Shared.
- Mục tiêu: giữ Domain thuần, hạn chế coupling.

Decision:
- Giữ layering như hiện tại:
	- Domain không tham chiếu EF/ASP.NET
	- Application định nghĩa contracts/interfaces (ports)
	- Infrastructure implement persistence/adapters
	- Api là composition root (DI, middleware, endpoints)
	- Shared chứa DTO/contracts cho public API payloads

Consequences:
- Controllers nên mỏng; mapping DTO tập trung.
- Nếu API cần truy vấn dữ liệu: ưu tiên qua Application interfaces thay vì gọi EF trực tiếp.

Status: Accepted
Date: 2026-01-03

---

## ADR-002 — Identity placement in Infrastructure
Context:
- `AppDbContext` kế thừa `IdentityDbContext`.
- Domain có `UserProfile/UserSettings` (business data) tách khỏi Identity tables.

Decision:
- `AppUser` và Identity wiring nằm ở Infrastructure.
- Domain chỉ giữ entity theo business (UserProfile/UserSettings) và join bằng `UserId: Guid`.

Consequences:
- Không để Domain phụ thuộc ASP.NET Identity.
- EF mapping/constraints được quản lý bởi Infrastructure.

Status: Accepted
Date: 2026-01-03

---

## ADR-003 — Auth strategy: JWT access token
Context:
- UI là SPA (Blazor WASM) gọi API qua HTTP.
- Hiện có `AuthController` register/login và `JwtTokenService`.

Decision:
- Dùng JWT Bearer access token (HMAC SHA-256) cho MVP.
- Lưu token phía UI trong browser storage (hiện dùng localStorage qua `window.__storage`).

Consequences:
- Cần chuẩn hoá claims (ít nhất `sub` và role claims).
- Nếu triển khai refresh token về sau: bổ sung endpoint/rotation + storage strategy.

Status: Accepted
Date: 2026-01-03

---

## ADR-004 — Configuration & secrets: user-secrets for Development
Context:
- `Api/Program.cs` load user-secrets khi Development.
- `Api/appsettings.Development.json` có chỗ trống cho `ConnectionStrings` và `Jwt:Key`.

Decision:
- Không commit secrets vào git.
- Mọi dev machine set bằng `dotnet user-secrets`.

Consequences:
- README + local-config phải hướng dẫn rõ cách set secrets.
- CI/CD (nếu có) cần inject secrets qua env vars hoặc secret store.

Status: Accepted
Date: 2026-01-03

---

## ADR-005 — Database: EF Core migrations + auto-migrate in Development
Context:
- API chạy `db.Database.MigrateAsync()` và seed trong Development (có retry).
- Migrations nằm trong Infrastructure.

Decision:
- Auto-migrate + seed chỉ trong Development.
- Non-development: migrate theo quy trình release (manual/CI step) để tránh "accidental schema change".

Consequences:
- Cần guard rõ môi trường để không migrate nhầm.
- Seed phải idempotent.

Status: Accepted
Date: 2026-01-03

---

## ADR-006 — API routing conventions
Context:
- Hiện có routes: `/api/auth/*`, `/api/me/*`, `/api/decks`, `/api/study/*`, `/api/questions/*`, `/api/ping`.

Decision:
- Chuẩn hoá REST conventions:
	- Resource plural nouns (decks, questions)
	- User-scoped resources dưới `/api/me/*`
	- Actions (non-CRUD) dùng subresource rõ ràng (study/next, study/answer)

Consequences:
- DTOs public nằm ở Shared.Contracts.
- Tránh return string error rời rạc; ưu tiên ProblemDetails.

Status: Proposed
Date: 2026-01-03

---

## ADR-007 — Error handling standard
Context:
- Controllers hiện return string cho BadRequest/Unauthorized/NotFound.
- UI đang `EnsureSuccessStatusCode()` nên message thường không thân thiện.

Decision:
- Thống nhất API error response bằng RFC7807 ProblemDetails (global exception handler + validation errors).

Consequences:
- UI có thể parse và hiển thị lỗi nhất quán.
- Giảm rò rỉ thông tin nội bộ.

Status: Proposed
Date: 2026-01-03

---

## ADR-008 — Logging & observability
Context:
- Hiện có Console.WriteLine cho dev diagnostics.
- Worker chỉ log heartbeat.

Decision:
- Chuẩn hoá structured logging (built-in ILogger với scopes/correlation id; Serilog optional).
- Add request logging middleware cho API.

Consequences:
- Dễ trace issue theo request/user.
- Sẵn sàng cho App Insights/OpenTelemetry sau này.

Status: Proposed
Date: 2026-01-03

---

## ADR-009 — Source of truth for question bank
Context:
- Có 2 nguồn dữ liệu:
	- SQL tables (Decks/Questions/QuestionOptions) dùng bởi `/api/decks` và `/api/study/*`.
	- Embedded question bank JSON dùng bởi `IDeckQueryService` và `/api/questions/{id}`.

Decision:
- Chọn 1 source-of-truth cho MVP (đề xuất: SQL tables) và refactor các endpoint query qua Application interface.

Consequences:
- Tránh UI thấy decks khác với endpoint questions.
- Seed/import trở nên rõ ràng.

Status: Proposed
Date: 2026-01-03
