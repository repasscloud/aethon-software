namespace Aethon.Shared.Jobs;

public sealed class JobApplicationListItemDto
{
    public string Id { get; set; } = "";
    public string JobId { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public string OrganisationName { get; set; } = "";
    public string Status { get; set; } = "";
    public DateTime SubmittedUtc { get; set; }
}
