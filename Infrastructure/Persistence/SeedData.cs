using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

/// <summary>
/// SeedData: tạo dữ liệu hệ thống tối thiểu để local dev/test chạy ổn định.
///
/// Nguyên tắc:
/// - Idempotent: chạy nhiều lần không tạo duplicate.
/// - Chỉ seed những thứ nhỏ và an toàn cho môi trường dev.
/// - MVP hiện tại chưa có TestVersion/Deck/ExamRule nên seed theo dạng key-value trong AppSettings.
/// </summary>
public static class SeedData
{
    /// <summary>
    /// Seed dữ liệu hệ thống tối thiểu.
    /// </summary>
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        // Đảm bảo DB đã được migrate trước khi seed.
        await db.Database.MigrateAsync(ct);

        // Các key này được dùng như "system constants" cho MVP.
        // Khi các bảng domain thật (Deck/Topic/...) được tạo, có thể chuyển seed sang các bảng đó.
        var desired = new Dictionary<string, string>
        {
            ["System.TestVersion"] = "2025.12",
            ["System.DefaultDeck"] = "ALL",
            ["System.DefaultDailyGoalMinutes"] = "15",
        };

        // Lấy tất cả key hiện có một lần để tối ưu.
        var existingKeys = await db.AppSettings
            .AsNoTracking()
            .Select(x => x.Key)
            .ToListAsync(ct);

        foreach (var kv in desired)
        {
            if (existingKeys.Contains(kv.Key))
                continue;

            db.AppSettings.Add(new AppSetting
            {
                Key = kv.Key,
                Value = kv.Value,
                CreatedAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
