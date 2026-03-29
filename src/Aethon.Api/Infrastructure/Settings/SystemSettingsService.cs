using Aethon.Application.Abstractions.Settings;
using Aethon.Data;
using Aethon.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Api.Infrastructure.Settings;

public sealed class SystemSettingsService : ISystemSettingsService
{
    private readonly AethonDbContext _db;

    public SystemSettingsService(AethonDbContext db)
    {
        _db = db;
    }

    public async Task<bool> GetBoolAsync(string key, bool defaultValue = false, CancellationToken ct = default)
    {
        var raw = await GetStringAsync(key, ct);
        if (raw is null) return defaultValue;
        return raw.Equals("true", StringComparison.OrdinalIgnoreCase) ||
               raw.Equals("1", StringComparison.Ordinal) ||
               raw.Equals("yes", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<string?> GetStringAsync(string key, CancellationToken ct = default)
    {
        var setting = await _db.SystemSettings.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Key == key, ct);
        return string.IsNullOrEmpty(setting?.Value) ? null : setting.Value;
    }

    public async Task SetAsync(string key, string value, string? description = null, Guid? updatedByUserId = null, CancellationToken ct = default)
    {
        var setting = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
        if (setting is null)
        {
            setting = new SystemSetting { Key = key };
            _db.SystemSettings.Add(setting);
        }

        setting.Value = value;
        setting.UpdatedUtc = DateTime.UtcNow;
        setting.UpdatedByUserId = updatedByUserId;
        if (description is not null)
            setting.Description = description;

        await _db.SaveChangesAsync(ct);
    }
}
