using Aethon.Data.Identity;

namespace Aethon.Data.Entities;

/// <summary>
/// Comment attached to a job application.
/// Supports threaded replies through ParentCommentId.
/// </summary>
public class JobApplicationComment : EntityBase
{
    /// <summary>
    /// The application this comment belongs to.
    /// </summary>
    public Guid JobApplicationId { get; set; }

    /// <summary>
    /// Navigation to the application.
    /// </summary>
    public JobApplication JobApplication { get; set; } = null!;

    /// <summary>
    /// Optional parent comment for threaded replies.
    /// </summary>
    public Guid? ParentCommentId { get; set; }

    /// <summary>
    /// Navigation to the parent comment.
    /// </summary>
    public JobApplicationComment? ParentComment { get; set; }

    /// <summary>
    /// Child replies to this comment.
    /// </summary>
    public ICollection<JobApplicationComment> Replies { get; set; } = new List<JobApplicationComment>();

    /// <summary>
    /// The comment body/content.
    /// </summary>
    public string Content { get; set; } = null!;

    /// <summary>
    /// Navigation to the comment creator.
    /// </summary>
    public ApplicationUser CreatedByUser { get; set; } = null!;

    /// <summary>
    /// Navigation to the user who last updated the comment.
    /// </summary>
    public ApplicationUser? UpdatedByUser { get; set; }

    /// <summary>
    /// Soft delete flag.
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// When the comment was deleted.
    /// </summary>
    public DateTime? DeletedUtc { get; set; }

    /// <summary>
    /// The user who deleted the comment.
    /// </summary>
    public Guid? DeletedByUserId { get; set; }

    /// <summary>
    /// Navigation to the user who deleted the comment.
    /// </summary>
    public ApplicationUser? DeletedByUser { get; set; }
}