# DECISIONS (Architecture Decision Records)

Project: Citizenship Tutor
Last updated: 2026-02-01

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
- Cần chuẩn hoá claims (ít nhất `sub`, `ClaimTypes.NameIdentifier` và role claims).
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

Status: Accepted (Implemented)
Date: 2026-01-03

---

## ADR-008 — Logging & observability
Context:
- Hiện API đã dùng ILogger; dev log thông tin an toàn (DataSource/Database) để debug cấu hình, không log secrets.
- WorkerService hiện chỉ log heartbeat (placeholder).

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
	- Embedded question bank JSON vẫn còn trong repo (legacy) nhưng không được wire vào DI/runtime.

Decision:
- Chọn 1 source-of-truth cho MVP (đề xuất: SQL tables) và refactor các endpoint query qua Application interface.

Consequences:
- Tránh UI thấy decks khác với endpoint questions.
- Seed/import trở nên rõ ràng.

Status: Accepted (Implemented)
Date: 2026-01-03

---

## ADR-010 — Integration test strategy for API
Context:
- Cần integration tests để khoá hành vi security + query endpoints.
- Test environment không nên phụ thuộc SQL Server thật (tăng friction, flaky trên CI/dev).

Decision:
- Dùng `WebApplicationFactory` + test auth scheme (header-driven) để kiểm soát auth/roles deterministically.
- Override `AppDbContext` trong test host sang EF Core InMemory provider và seed dữ liệu tối thiểu cho test.

Consequences:
- Tests chạy nhanh, ổn định, không cần external DB.
- Lưu ý: EF InMemory không phản ánh hoàn toàn SQL semantics; các test về query phức tạp/constraint nên bổ sung layer test khác (SQLite/SQLServer) khi cần.

Status: Accepted (Implemented)
Date: 2026-01-04

---

## ADR-011 — Documentation source of truth is `/docs`
Context:
- Repo đang có các file docs bị nhân bản (root + /docs) dễ gây cập nhật nhầm.

Decision:
- Chọn **`/docs`** là nguồn sự thật duy nhất cho mọi tài liệu.
- Các file trùng ở repo root (nếu còn giữ) phải là **stub/pointer** trỏ về `/docs/*`.

Consequences:
- Giảm drift giữa code và tài liệu.
- Khi review PR, chỉ cần kiểm tra `/docs` để biết trạng thái thật.

Status: Accepted (Implemented)
Date: 2026-01-16

---

## ADR-012 — JWT must include `ClaimTypes.NameIdentifier`
Context:
- Nhiều controller parse userId từ claim `ClaimTypes.NameIdentifier`.
- Nếu chỉ dựa vào claim mapping mặc định của JwtBearer, có thể xảy ra lệch giữa môi trường.

Decision:
- Khi tạo JWT, luôn đặt đồng thời:
  - `sub` (JwtRegisteredClaimNames.Sub)
  - `ClaimTypes.NameIdentifier`

Consequences:
- Parse userId thống nhất và không phụ thuộc inbound claim mapping.
- Test auth scheme dễ mô phỏng hành vi thật.

Status: Accepted (Implemented)
Date: 2026-01-14

---

## ADR-013 — CORS non-development uses safe default (deny if not configured)
Context:
- Với SPA, CORS thường chỉ nên mở cho domain UI đã biết.
- Nếu cấu hình thiếu, việc “AllowAnyOrigin” trong production là rủi ro.

Decision:
- Development: AllowAnyOrigin/AnyHeader/AnyMethod.
- Non-development:
  - Đọc `Cors:AllowedOrigins`.
  - Nếu không cấu hình origins, **deny cross-origin** (không crash, không allow-all).

Consequences:
- Tránh mở CORS nhầm trong production.
- Deploy cần cấu hình rõ allowed origins.

Status: Accepted (Implemented)
Date: 2026-01-14

---

## ADR-014 — Reverse proxy headers are opt-in via `Proxy:Enabled`
Context:
- Khi chạy sau nginx/cloudflared, API cần tôn trọng `X-Forwarded-Proto/For`.
- Tuy nhiên nếu bật mặc định, có thể mở trust boundary không mong muốn.

Decision:
- Chỉ bật forwarded headers khi `Proxy:Enabled=true`.
- Mặc định tắt để an toàn.

Consequences:
- Deploy sau reverse proxy phải bật cấu hình này.
- Giảm rủi ro spoof header khi API không nằm sau proxy.

Status: Accepted (Implemented)
Date: 2026-01-14

---

## ADR-015 — Separate liveness and readiness health checks
Context:
- Cần endpoint cho orchestrator/proxy biết process có chạy và app có sẵn sàng phục vụ hay chưa.

Decision:
- Expose:
  - `GET /health/live`: chỉ check “self”
  - `GET /health/ready`: check “self” + DB connectivity

Consequences:
- Proxy/load balancer có thể dùng readiness để tránh route traffic khi DB down.

Status: Accepted (Implemented)
Date: 2026-01-14

---

## ADR-016 — Full settings contract: `UserSettingContracts` uses properties + DataAnnotations
Context:
- App cần payload "full settings" (Language/FontScale/AudioSpeed/DailyGoalMinutes/Focus/SilentMode).
- Blazor UI cần 2 yêu cầu:
  - `@bind` trên `<select>/<input>` phải gán được giá trị (cần setter).
  - API model validation phải đọc được DataAnnotations ổn định.
- Positional record (`record Foo(...)`) dễ gặp cảnh báo/behavior: validation metadata gắn trên property có thể bị bỏ qua.

Decision:
- Dùng DTO `Shared.Contracts.Me.UserSettingContracts` dưới dạng **property-based record**.
- Các properties dùng **DataAnnotations** và có **setter** (`get; set;`) để Blazor binding hoạt động.

Consequences:
- UI có thể bind trực tiếp vào DTO để edit settings (ít mapping, nhanh cho MVP).
- DTO không còn hoàn toàn immutable; nếu muốn immutable về sau, có thể thêm ViewModel "edit model" riêng cho UI.

Status: Accepted (Implemented)
Date: 2026-01-18

---

## ADR-017 — Add `/api/me/settings/full` while keeping MVP `/api/me/settings`
Context:
- Trước đó UI Settings chỉ cần 2 fields (Language + DailyGoalMinutes) qua `/api/me/settings`.
- Step 5 yêu cầu full settings + onboarding cần save nhiều fields.
- Muốn thêm full settings mà không phá backward compatibility (các page/clients cũ).

Decision:
- Giữ endpoint MVP:
  - `GET/PUT /api/me/settings` (2 fields)
- Thêm endpoint full:
  - `GET/PUT /api/me/settings/full` (full payload `UserSettingContracts`)

Consequences:
- UI Settings/Onboarding mới dùng endpoint `/full`.
- Có thể deprecate endpoint MVP khi không còn dùng (future cleanup).

Status: Deprecated (see ADR-023)
Date: 2026-01-18

# Architectural Decisions

---

## ADR-023 — Remove legacy MVP settings endpoint

**Context**
The original `/api/me/settings` endpoint supported only partial settings and
duplicated logic after full settings were introduced.

**Decision**
- Remove legacy endpoint and contracts
- Use `/api/me/settings/full` as the single source of truth

**Consequences**
- Reduced API surface
- No duplicated logic
- Clear ownership of settings behavior

**Status**: Accepted
**Date**: 2026-01-19


---

## ADR-024: Paging parameters for deck question lists

**Status:** Accepted
**Date:** 2026-02-01

### Decision
For endpoints returning potentially large question lists (starting with deck questions), we use query-string paging:

- `page` (1-based)
- `pageSize` (bounded)

Example:
- `GET /api/decks/{deckId}/questions?page=1&pageSize=50`

Validation rules:
- `page >= 1`
- `1 <= pageSize <= 200`

Ordering rule:
- Always order by a stable key (`QuestionId`) before applying `Skip/Take` to ensure deterministic paging.

### Rationale
- Keeps payload sizes predictable.
- Avoids accidental “load all questions” behavior.
- Stable ordering prevents duplicates/missing items when paging.

---

## ADR-025: Separate SystemLanguage vs content Language

**Status:** Accepted
**Date:** 2026-02-01

### Decision
User settings store two language values:

- `SystemLanguage`: UI/system text language (default `Vi` for the target audience).
- `Language`: question content language (default `En`).

UI edits both, and the API persists both in `UserSettings`.

### Rationale
- Elderly learners often want Vietnamese UI, but still practice English questions.
- Keeps localization logic explicit and avoids overloading a single “language” flag.

