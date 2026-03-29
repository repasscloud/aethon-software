namespace Aethon.Shared.CompanyRecruiters;

public sealed class CreateCompanyRecruiterInviteDto
{
    public Guid RecruiterOrganisationId { get; set; }

    public bool AllowCreateDraftJobs { get; set; }
    public bool AllowSubmitJobsForApproval { get; set; }
    public bool AllowManageApprovedJobs { get; set; }
    public bool AllowViewCandidates { get; set; }
    public bool AllowSubmitCandidates { get; set; }
    public bool AllowCommunicateWithCandidates { get; set; }
    public bool AllowScheduleInterviews { get; set; }
    public bool AllowPublishJobs { get; set; }

    public bool RecruiterCanCreateUnclaimedCompanyJobs { get; set; }
    public bool RecruiterCanPublishJobs { get; set; }
    public bool RecruiterCanManageCandidates { get; set; }

    public string? Message { get; set; }
}
