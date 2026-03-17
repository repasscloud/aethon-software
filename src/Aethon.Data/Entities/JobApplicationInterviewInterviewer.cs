using Aethon.Data.Identity;

namespace Aethon.Data.Entities;

/// <summary>
/// Links an interviewer user to a scheduled application interview.
/// </summary>
public class JobApplicationInterviewInterviewer : EntityBase
{
    /// <summary>
    /// The interview this assignment belongs to.
    /// </summary>
    public Guid JobApplicationInterviewId { get; set; }

    /// <summary>
    /// Navigation to the interview.
    /// </summary>
    public JobApplicationInterview JobApplicationInterview { get; set; } = null!;

    /// <summary>
    /// The assigned interviewer user.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigation to the assigned interviewer.
    /// </summary>
    public ApplicationUser User { get; set; } = null!;

    /// <summary>
    /// Optional role/label for the interviewer.
    /// Example: Hiring Manager, Technical Interviewer, Recruiter
    /// </summary>
    public string? RoleLabel { get; set; }
}