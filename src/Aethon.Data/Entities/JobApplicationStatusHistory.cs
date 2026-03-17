using Aethon.Data.Identity;
using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

/// <summary>
/// Historical record of status changes for a job application.
/// This is the audit/timeline source of truth for application workflow movement.
/// </summary>
public class JobApplicationStatusHistory : EntityBase
{
    /// <summary>
    /// The application this history item belongs to.
    /// </summary>
    public Guid JobApplicationId { get; set; }

    /// <summary>
    /// Navigation to the application.
    /// </summary>
    public JobApplication JobApplication { get; set; } = null!;

    /// <summary>
    /// Previous application status, if any.
    /// Null can be used for the very first workflow entry.
    /// </summary>
    public ApplicationStatus? FromStatus { get; set; }

    /// <summary>
    /// New application status.
    /// </summary>
    public ApplicationStatus ToStatus { get; set; }

    /// <summary>
    /// Optional reason for the status change.
    /// Example: "Did not meet required experience"
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Optional internal note explaining the change.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// The user who performed the status change.
    /// </summary>
    public Guid ChangedByUserId { get; set; }

    /// <summary>
    /// Navigation to the user who changed the status.
    /// </summary>
    public ApplicationUser ChangedByUser { get; set; } = null!;

    /// <summary>
    /// When the status change occurred.
    /// </summary>
    public DateTime ChangedUtc { get; set; }
}