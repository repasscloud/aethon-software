namespace Aethon.Shared.Jobs;

public sealed class JobApplicationResultDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string Status { get; set; } = "";
    public DateTime SubmittedUtc { get; set; }
}
