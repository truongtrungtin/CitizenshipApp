using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Identity;

/// <summary>
///     AppUser là Identity user chính của hệ thống.
///     - Đặt ở Infrastructure theo quyết định: Identity belongs to Infrastructure.
///     - Guid key để dễ đồng bộ với các bảng domain (UserId).
/// </summary>
public sealed class AppUser : IdentityUser<Guid>
{
    // MVP: chưa cần thêm fields.
    // Nếu cần thêm: FullName, PhoneVerified, etc. có thể thêm sau.
}
