namespace Aethon.Data.Entities;

/// <summary>
/// Persistent application log entry written by <c>ISystemLogService</c>.
/// Used for admin-visible diagnostics, error tracking, and operational alerts.
/// </summary>
public class SystemLog
{
    public Guid Id { get; set; }

    /// <summary>UTC timestamp of when the event occurred.</summary>
    public DateTime TimestampUtc { get; set; }

    /// <summary>Severity level (Debug / Info / Warning / Error / Critical).</summary>
    public SystemLogLevel Level { get; set; }

    /// <summary>Short source identifier, e.g. "StripeWebhook", "Auth", "Billing".</summary>
    public string Category { get; set; } = "";

    /// <summary>Human-readable message describing what happened.</summary>
    public string Message { get; set; } = "";

    /// <summary>Optional additional context — free text or structured JSON.</summary>
    public string? Details { get; set; }

    /// <summary>Exception type name if an exception was captured.</summary>
    public string? ExceptionType { get; set; }

    /// <summary>Exception message if an exception was captured.</summary>
    public string? ExceptionMessage { get; set; }

    /// <summary>UserId of the actor, if the event was triggered by an authenticated user.</summary>
    public Guid? UserId { get; set; }

    /// <summary>HTTP request path that triggered the event, if applicable.</summary>
    public string? RequestPath { get; set; }
}
