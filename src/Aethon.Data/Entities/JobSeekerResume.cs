namespace Aethon.Data.Entities;

/// <summary>
/// A resume/CV uploaded by a job seeker.
/// A candidate can keep multiple resumes and choose which one to use when applying.
/// </summary>
public class JobSeekerResume : EntityBase
{
    /// <summary>
    /// The profile this resume belongs to.
    /// </summary>
    public Guid JobSeekerProfileId { get; set; }

    /// <summary>
    /// Navigation to the owning job seeker profile.
    /// </summary>
    public JobSeekerProfile JobSeekerProfile { get; set; } = null!;

    /// <summary>
    /// The stored file for this resume.
    /// </summary>
    public Guid StoredFileId { get; set; }

    /// <summary>
    /// Navigation to the stored file.
    /// </summary>
    public StoredFile StoredFile { get; set; } = null!;

    /// <summary>
    /// Friendly name shown to the user.
    /// Example: "General Resume", "Senior .NET Resume", "Project Manager CV"
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Optional description to help the candidate distinguish resume versions.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Indicates this is the default resume for quick apply flows.
    /// </summary>
    public bool IsDefault { get; set; }

    /// <summary>
    /// Indicates the resume is active and selectable.
    /// Allows soft-retiring older versions without deleting them.
    /// </summary>
    public bool IsActive { get; set; } = true;
}