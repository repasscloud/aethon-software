namespace Aethon.Shared.Applications;

public sealed class ApplicationAssignmentDto
{
    public Guid ApplicationId { get; set; }
    public Guid? AssignedRecruiterUserId { get; set; }
    public Guid? AssignedRecruiterOrganisationId { get; set; }
    public DateTime? AssignedUtc { get; set; }
}
