namespace Aethon.Application.Candidates.Commands.AddCandidateResume;

public sealed class AddCandidateResumeCommand
{
    public Guid StoredFileId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsDefault { get; init; }
}
