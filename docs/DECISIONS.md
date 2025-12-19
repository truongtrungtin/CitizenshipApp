# DECISIONS

Project: **App ôn thi quốc tịch Mỹ cho người lớn tuổi**
Last updated: **2025-12-17**

> Quy ước: mỗi decision có (Decision / Why / Impact).

## 2025-12-17 — UI framework: Blazor WebAssembly
**Decision:** Dùng **Blazor WebAssembly** cho web UI.
**Why:** Tốc độ MVP nhanh trong .NET; dễ reuse để lên MAUI Blazor Hybrid.
**Impact:** Tách project `Ui.Blazor` và `Shared` (DTO + ApiClient) để tái sử dụng.

## 2025-12-17 — Admin: Swagger-only
**Decision:** Không làm UI admin, dùng **Swagger**.
**Why:** Tiết kiệm thời gian; ưu tiên UX end-user.
**Impact:** `/admin/*` endpoints bắt buộc Admin policy/role; Swagger bật Bearer.

## 2025-12-17 — Auth: JWT (refresh optional)
**Decision:** Dùng **JWT**; refresh token optional theo thời gian.
**Why:** Dễ test, đơn giản cho self-host.
**Impact:** `/auth/login` trả JWT; UI attach Authorization header.

## 2025-12-17 — Onboarding required
**Decision:** Sau register phải onboarding (StateCode, FontScale, PreferredLanguage).
**Why:** Tránh user bỏ sót setting quan trọng, tối ưu trải nghiệm người lớn tuổi.
**Impact:** `UserProfile.IsOnboarded` + route guard UI.

## 2025-12-17 — Audio caching strategy
**Decision:** Audio lưu **file** theo `ContentHash`; DB lưu metadata.
**Why:** DB nhẹ, load nhanh, dễ backup.
**Impact:** Thư mục `data/audio/{hash}.mp3`, serve static/endpoint với cache headers.

## 2025-12-17 — AI generation strategy
**Decision:** Generate phonetic/explain **một lần** bằng admin, lưu DB.
**Why:** Giảm chi phí vận hành, tăng tốc end-user.
**Impact:** `AiGenerationJob/Item` + worker; `QuestionText/AnswerText` lưu nội dung + reviewed flags.

## 2025-12-17 — Silent mode behavior
**Decision:** Tap “Tạm thời tôi không thể nói hoặc nghe” → khóa session sang MCQ-only.
**Why:** UX đơn giản, phù hợp môi trường không nghe/nói.
**Impact:** `StudySession.SilentMode` + `StudySession.ForceMcqOnly`.


## 2025-12-17 — Identity model placement (Infrastructure)
**Decision:** `AppUser` (Identity entity) đặt trong **Infrastructure**, còn `UserProfile/UserSettings` nằm trong **Domain**.
**Why:** Tránh Domain phụ thuộc ASP.NET Identity; Domain giữ “business data” thuần.
**Impact:** `AppDbContext : IdentityDbContext<AppUser,...>` nằm trong Infrastructure; liên kết bằng `UserId (Guid)` + unique index.


## 2025-12-18 — Swagger/OpenAPI v10+ compatibility + temporary API key gate
Decision: Use Swashbuckle.AspNetCore v10.x (OpenAPI.NET v2+) for Swagger generation and update code to `Microsoft.OpenApi` types/APIs. Protect `/api/*` routes (dev/MVP) using an `X-Admin-Key` header middleware and expose this header in Swagger UI.
Why: Fixes compile/runtime errors caused by OpenAPI type/namespace mismatch and allows testing protected endpoints via Swagger before JWT/role-based auth is implemented.
Impact: Program.cs uses `AddSecurityRequirement(doc => ...)` + `OpenApiSecuritySchemeReference`. Future auth work can replace the API-key gate with JWT + role/policy protection on `/admin`.

Last updated: **2025-12-18**
