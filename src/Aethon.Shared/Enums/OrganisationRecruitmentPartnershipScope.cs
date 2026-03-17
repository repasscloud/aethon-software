namespace Aethon.Shared.Enums;

[Flags]
public enum OrganisationRecruitmentPartnershipScope
{
    None = 0,
    CreateDraftJobs = 1 << 0,
    SubmitJobsForApproval = 1 << 1,
    ManageApprovedJobs = 1 << 2,
    ViewCandidates = 1 << 3,
    SubmitCandidates = 1 << 4,
    CommunicateWithCandidates = 1 << 5,
    ScheduleInterviews = 1 << 6,
    PublishJobs = 1 << 7
}