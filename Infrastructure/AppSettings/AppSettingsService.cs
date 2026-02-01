using Application.AppSettings;

using Infrastructure.Persistence;

using Microsoft.EntityFrameworkCore;

using Shared.Contracts.AppSettings;

namespace Infrastructure.AppSettings;

/// <summary>
/// Infrastructure implementation for AppSettings operations.
/// </summary>
public sealed class AppSettingsService(AppDbContext db) : IAppSettingsService
{
    public async Task<List<AppSettingDto>> GetAllAsync(CancellationToken ct)
    {
        return await db.AppSettings
            .AsNoTracking()
            .OrderByDescending(x => x.Id)
            .Select(x => new AppSettingDto(x.Id, x.Key, x.Value))
            .ToListAsync(ct);
    }

    public async Task<AppSettingDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        return await db.AppSettings
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new AppSettingDto(x.Id, x.Key, x.Value))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<AppSettingResult> CreateAsync(CreateAppSettingRequest req, CancellationToken ct)
    {
        bool exists = await db.AppSettings.AsNoTracking().AnyAsync(x => x.Key == req.Key, ct);
        if (exists)
        {
            return AppSettingResult.Fail(AppSettingFailureReason.Conflict, "Key already exists.");
        }

        var entity = new Domain.Entities.AppSetting
        {
            Key = req.Key,
            Value = req.Value
        };

        db.AppSettings.Add(entity);
        await db.SaveChangesAsync(ct);

        return AppSettingResult.Success(new AppSettingDto(entity.Id, entity.Key, entity.Value));
    }

    public async Task<AppSettingResult> UpdateAsync(int id, UpdateAppSettingRequest req, CancellationToken ct)
    {
        Domain.Entities.AppSetting? item = await db.AppSettings.FindAsync([id], ct);
        if (item is null)
        {
            return AppSettingResult.Fail(AppSettingFailureReason.NotFound, "App setting not found.");
        }

        bool keyInUseByOther = await db.AppSettings.AsNoTracking()
            .AnyAsync(x => x.Key == req.Key && x.Id != id, ct);
        if (keyInUseByOther)
        {
            return AppSettingResult.Fail(AppSettingFailureReason.Conflict, "Key already exists.");
        }

        item.Key = req.Key;
        item.Value = req.Value;
        await db.SaveChangesAsync(ct);

        return AppSettingResult.Success(new AppSettingDto(item.Id, item.Key, item.Value));
    }

    public async Task<AppSettingResult> DeleteAsync(int id, CancellationToken ct)
    {
        Domain.Entities.AppSetting? item = await db.AppSettings.FindAsync([id], ct);
        if (item is null)
        {
            return AppSettingResult.Fail(AppSettingFailureReason.NotFound, "App setting not found.");
        }

        db.Remove(item);
        await db.SaveChangesAsync(ct);
        return AppSettingResult.Success();
    }
}
