namespace Aethon.Shared.Jobs;

public sealed class JobApplicationResultDto
{
    public string Id { get; set; } = "";
    public string JobId { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime SubmittedUtc { get; set; }
}
