using Aethon.Shared.Enums;

namespace Aethon.Data.Entities;

/// <summary>
/// Interview scheduled for a job application.
/// </summary>
public class JobApplicationInterview : EntityBase
{
    /// <summary>
    /// The application this interview belongs to.
    /// </summary>
    public Guid JobApplicationId { get; set; }

    /// <summary>
    /// Navigation to the application.
    /// </summary>
    public JobApplication JobApplication { get; set; } = null!;

    /// <summary>
    /// Type of interview.
    /// </summary>
    public InterviewType Type { get; set; }

    /// <summary>
    /// Current interview status.
    /// </summary>
    public InterviewStatus Status { get; set; }

    /// <summary>
    /// Optional title for the interview.
    /// Example: "Technical Interview - Round 1"
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Optional location text.
    /// Example: office address, "Zoom", "Google Meet"
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Optional meeting URL for remote interviews.
    /// </summary>
    public string? MeetingUrl { get; set; }

    /// <summary>
    /// Optional free-text notes or agenda.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Scheduled start time in UTC.
    /// </summary>
    public DateTime ScheduledStartUtc { get; set; }

    /// <summary>
    /// Scheduled end time in UTC.
    /// </summary>
    public DateTime ScheduledEndUtc { get; set; }

    /// <summary>
    /// When the interview was completed, if applicable.
    /// </summary>
    public DateTime? CompletedUtc { get; set; }

    /// <summary>
    /// When the interview was cancelled, if applicable.
    /// </summary>
    public DateTime? CancelledUtc { get; set; }

    /// <summary>
    /// Optional reason if the interview was cancelled.
    /// </summary>
    public string? CancellationReason { get; set; }

    /// <summary>
    /// Users assigned as interviewers for this interview.
    /// </summary>
    public ICollection<JobApplicationInterviewInterviewer> Interviewers { get; set; } = new List<JobApplicationInterviewInterviewer>();
}