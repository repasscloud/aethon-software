using Aethon.Data.Identity;

namespace Aethon.Data.Entities;

/// <summary>
/// Internal note attached to a job application.
/// Intended for recruiter/company-side notes, not candidate-visible content.
/// </summary>
public class JobApplicationNote : EntityBase
{
    /// <summary>
    /// The application this note belongs to.
    /// </summary>
    public Guid JobApplicationId { get; set; }

    /// <summary>
    /// Navigation to the application.
    /// </summary>
    public JobApplication JobApplication { get; set; } = null!;

    /// <summary>
    /// The note body/content.
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Navigation to the note creator.
    /// </summary>
    public ApplicationUser CreatedByUser { get; set; } = null!;

    /// <summary>
    /// Navigation to the user who last updated the note.
    /// </summary>
    public ApplicationUser? UpdatedByUser { get; set; }

    /// <summary>
    /// Soft delete flag for hiding notes without losing history.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the note was deleted, if soft deleted.
    /// </summary>
    public DateTime? DeletedUtc { get; set; }

    /// <summary>
    /// The user who deleted the note, if soft deleted.
    /// </summary>
    public Guid? DeletedByUserId { get; set; }

    /// <summary>
    /// Navigation to the user who deleted the note.
    /// </summary>
    public ApplicationUser? DeletedByUser { get; set; }
}