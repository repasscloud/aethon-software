namespace Aethon.Application.Applications.Commands.SubmitJobApplication;

public sealed class SubmitJobApplicationCommand
{
    public Guid JobId { get; init; }
    public Guid? ResumeFileId { get; init; }
    public string? CoverLetter { get; init; }
    public string? Source { get; init; }
    public string? ScreeningAnswersJson { get; init; }
}
