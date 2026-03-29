namespace Aethon.Application.Abstractions.Logging;

/// <summary>
/// Writes structured log entries to the persistent <c>SystemLogs</c> DB table
/// for admin-visible diagnostics and operational monitoring.
/// Inject this into any service that needs auditable, queryable logging.
/// </summary>
public interface ISystemLogService
{
    Task LogAsync(
        SystemLogLevel level,
        string category,
        string message,
        string? details = null,
        Guid? userId = null,
        Exception? exception = null,
        string? requestPath = null,
        CancellationToken ct = default);

    Task InfoAsync(string category, string message, string? details = null, CancellationToken ct = default);
    Task WarnAsync(string category, string message, string? details = null, CancellationToken ct = default);
    Task ErrorAsync(string category, string message, string? details = null, Exception? exception = null, CancellationToken ct = default);
    Task CriticalAsync(string category, string message, string? details = null, Exception? exception = null, CancellationToken ct = default);
}
