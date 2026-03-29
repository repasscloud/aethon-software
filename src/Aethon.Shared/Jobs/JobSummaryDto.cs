namespace Aethon.Shared.Jobs;

public sealed class JobSummaryDto
{
    public Guid Id { get; set; }
    public Guid CompanyOrganisationId { get; set; }
    public Guid? ManagedByRecruiterOrganisationId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OrganisationName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
    public DateTime? SubmittedForApprovalUtc { get; set; }
    public DateTime? ApprovedUtc { get; set; }
    public DateTime? PublishedUtc { get; set; }
}
