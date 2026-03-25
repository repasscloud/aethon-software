using Aethon.Application.Abstractions.Logging;
using Aethon.Data;
using Aethon.Data.Entities;

namespace Aethon.Api.Infrastructure.Logging;

/// <summary>
/// Writes log entries to the <c>SystemLogs</c> database table.
/// Registered as scoped — one instance per HTTP request.
/// </summary>
public sealed class SystemLogService : ISystemLogService
{
    private readonly AethonDbContext _db;

    public SystemLogService(AethonDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(
        SystemLogLevel level,
        string category,
        string message,
        string? details = null,
        Guid? userId = null,
        Exception? exception = null,
        string? requestPath = null,
        CancellationToken ct = default)
    {
        _db.SystemLogs.Add(new SystemLog
        {
            Id               = Guid.NewGuid(),
            TimestampUtc     = DateTime.UtcNow,
            Level            = level,
            Category         = category,
            Message          = message,
            Details          = details,
            ExceptionType    = exception?.GetType().Name,
            ExceptionMessage = exception?.Message,
            UserId           = userId,
            RequestPath      = requestPath
        });

        await _db.SaveChangesAsync(ct);
    }

    public Task InfoAsync(string category, string message, string? details = null, CancellationToken ct = default)
        => LogAsync(SystemLogLevel.Info, category, message, details, ct: ct);

    public Task WarnAsync(string category, string message, string? details = null, CancellationToken ct = default)
        => LogAsync(SystemLogLevel.Warning, category, message, details, ct: ct);

    public Task ErrorAsync(string category, string message, string? details = null, Exception? exception = null, CancellationToken ct = default)
        => LogAsync(SystemLogLevel.Error, category, message, details, exception: exception, ct: ct);

    public Task CriticalAsync(string category, string message, string? details = null, Exception? exception = null, CancellationToken ct = default)
        => LogAsync(SystemLogLevel.Critical, category, message, details, exception: exception, ct: ct);
}
