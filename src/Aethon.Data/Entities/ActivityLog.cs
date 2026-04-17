using Aethon.Data.Identity;

namespace Aethon.Data.Entities;

/// <summary>
/// Generic activity/audit record across the system.
/// Can be used for jobs, applications, organisations, relationships, and more.
/// </summary>
public class ActivityLog : EntityBase
{
    /// <summary>
    /// The organisation context this activity belongs to, if applicable.
    /// </summary>
    public Guid? OrganisationId { get; set; }

    /// <summary>
    /// Navigation to the organisation context.
    /// </summary>
    public Organisation? Organisation { get; set; }

    /// <summary>
    /// Type of entity affected.
    /// Example: Job, JobApplication, Organisation, OrganisationMembership
    /// </summary>
    public string EntityType { get; set; } = null!;

    /// <summary>
    /// Id of the affected entity.
    /// </summary>
    public Guid EntityId { get; set; }

    /// <summary>
    /// Action performed.
    /// Example: Created, Updated, Assigned, Approved, Rejected, Withdrawn
    /// </summary>
    public string Action { get; set; } = null!;

    /// <summary>
    /// Optional short summary of what occurred.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Optional longer detail payload.
    /// Store structured JSON later if you want.
    /// </summary>
    public string? Details { get; set; }

    /// <summary>
    /// The user who performed the action, if applicable.
    /// </summary>
    public Guid? PerformedByUserId { get; set; }

    /// <summary>
    /// Navigation to the user who performed the action.
    /// </summary>
    public ApplicationUser? PerformedByUser { get; set; }

    /// <summary>
    /// When the action occurred.
    /// </summary>
    public DateTime PerformedUtc { get; set; }
}