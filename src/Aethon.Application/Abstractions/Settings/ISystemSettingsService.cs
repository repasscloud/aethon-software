namespace Aethon.Application.Abstractions.Settings;

public interface ISystemSettingsService
{
    Task<bool> GetBoolAsync(string key, bool defaultValue = false, CancellationToken ct = default);
    Task<string?> GetStringAsync(string key, CancellationToken ct = default);
    Task SetAsync(string key, string value, string? description = null, Guid? updatedByUserId = null, CancellationToken ct = default);
}
