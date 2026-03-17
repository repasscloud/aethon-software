using Aethon.Data.Identity;
using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

public class JobApplication : EntityBase
{
    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;

    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public ApplicationStatus Status { get; set; }

    public string? ResumeFileId { get; set; }
    public string? CoverLetter { get; set; }

    public DateTime SubmittedUtc { get; set; }
    public DateTime? LastStatusChangedUtc { get; set; }

    public string? Source { get; set; }
    public string? Notes { get; set; }
}