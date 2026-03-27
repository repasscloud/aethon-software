using Aethon.Data.Identity;
using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

/// <summary>
/// A user's request to have their identity verified.
/// Submitted via the verification request form.
/// Processed by admin staff or an org owner, or by the IdentityVerificationWorker.
/// Only one Pending request may exist per user at a time.
/// Requires the user's email to be confirmed (EmailConfirmed = true) before submission.
/// </summary>
public class IdentityVerificationRequest : EntityBase
{
    /// <summary>The user submitting the request.</summary>
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    /// <summary>Full legal name as provided by the user.</summary>
    public string FullName { get; set; } = null!;

    /// <summary>Email address provided by the user — must match their confirmed account email.</summary>
    public string EmailAddress { get; set; } = null!;

    /// <summary>Phone number provided by the user for verification.</summary>
    public string PhoneNumber { get; set; } = null!;

    /// <summary>Any additional context the user wants to provide.</summary>
    public string? AdditionalNotes { get; set; }

    /// <summary>Current processing status of this request.</summary>
    public VerificationRequestStatus Status { get; set; } = VerificationRequestStatus.Pending;

    /// <summary>The user who reviewed this request (admin, org owner, or null if system-processed).</summary>
    public Guid? ReviewedByUserId { get; set; }
    public ApplicationUser? ReviewedByUser { get; set; }

    /// <summary>When the request was reviewed.</summary>
    public DateTime? ReviewedUtc { get; set; }

    /// <summary>Internal notes from the reviewer explaining the decision.</summary>
    public string? ReviewNotes { get; set; }

    /// <summary>Indicates who or what reviewed this request.</summary>
    public VerificationReviewerType? ReviewerType { get; set; }
}
