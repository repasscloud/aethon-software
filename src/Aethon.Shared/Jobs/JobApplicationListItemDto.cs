namespace Aethon.Shared.Jobs;

public sealed class JobApplicationListItemDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }
    public string JobTitle { get; set; } = "";
    public string OrganisationName { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime SubmittedUtc { get; set; }
}
