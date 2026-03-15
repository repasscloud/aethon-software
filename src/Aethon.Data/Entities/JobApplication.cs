using Aethon.Data.Enums;
using Aethon.Data.Identity;

namespace Aethon.Data.Entities;

public class JobApplication : EntityBase
{
    public string JobId { get; set; } = null!;
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