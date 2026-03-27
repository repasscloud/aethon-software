using Aethon.Application.Abstractions.Integrations;
using Aethon.Application.Activity.Services;
using Aethon.Application.Applications.Commands.AddApplicationComment;
using Aethon.Application.Applications.Commands.AddApplicationNote;
using Aethon.Application.Applications.Commands.AttachApplicationFile;
using Aethon.Application.Applications.Commands.ChangeApplicationStatus;
using Aethon.Application.Applications.Commands.ScheduleInterview;
using Aethon.Application.Applications.Commands.SubmitJobApplication;
using Aethon.Application.Applications.Queries.GetApplicationById;
using Aethon.Application.Applications.Queries.GetApplicationFiles;
using Aethon.Application.Applications.Queries.GetApplicationTimeline;
using Aethon.Application.Applications.Queries.GetApplicationsForJob;
using Aethon.Application.Applications.Queries.GetMyApplications;
using Aethon.Application.Applications.Services;
using Aethon.Application.Candidates.Commands.AddCandidateResume;
using Aethon.Application.Candidates.Commands.RemoveCandidateResume;
using Aethon.Application.Candidates.Commands.SetDefaultCandidateResume;
using Aethon.Application.Candidates.Commands.TriggerResumeAnalysis;
using Aethon.Application.Candidates.Commands.UpsertMyCandidateProfile;
using Aethon.Application.Candidates.Queries.GetMyCandidateProfile;
using Aethon.Application.Candidates.Queries.GetPublicJobSeekerProfile;
using Aethon.Application.Candidates.Queries.GetResumeAnalysis;
using Aethon.Application.Files.Commands.UploadStoredFile;
using Aethon.Application.Integrations.Commands.CreateWebhookSubscription;
using Aethon.Application.Integrations.Queries.GetWebhookSubscriptions;
using Aethon.Application.Integrations.Services;
using Aethon.Application.Jobs.Commands.CloseJob;
using Aethon.Application.Jobs.Commands.CreateJob;
using Aethon.Application.Jobs.Commands.EmailJobApplication;
using Aethon.Application.Jobs.Commands.PublishJob;
using Aethon.Application.Jobs.Commands.PutJobOnHold;
using Aethon.Application.Jobs.Commands.ReturnJobToDraft;
using Aethon.Application.Jobs.Commands.UpdateJob;
using Aethon.Application.Jobs.Queries.GetJobById;
using Aethon.Application.Jobs.Queries.GetMyOrgJobs;
using Aethon.Application.Jobs.Queries.GetPublicJobDetail;
using Aethon.Application.Jobs.Queries.GetPublicJobLocations;
using Aethon.Application.Jobs.Queries.GetPublicJobs;
using Aethon.Application.Organisations.Commands.AcceptOrganisationInvite;
using Aethon.Application.Organisations.Commands.AddOrganisationDomain;
using Aethon.Application.Organisations.Commands.CancelClaimRequest;
using Aethon.Application.Organisations.Commands.ConfirmDomainVerification;
using Aethon.Application.Organisations.Commands.CreateOrganisationInvite;
using Aethon.Application.Organisations.Commands.RegenerateDomainVerificationToken;
using Aethon.Application.Organisations.Commands.RemoveOrganisationMember;
using Aethon.Application.Organisations.Commands.SubmitOrganisationClaim;
using Aethon.Application.Organisations.Commands.UpdateMemberRole;
using Aethon.Application.Organisations.Commands.UpdateMemberStatus;
using Aethon.Application.Organisations.Commands.UpdateMyDisplayName;
using Aethon.Application.Organisations.Commands.UpdateMyOrganisationProfile;
using Aethon.Application.Organisations.Commands.UpsertMyMemberProfile;
using Aethon.Application.Organisations.Commands.VerifyMemberIdentity;
using Aethon.Application.Organisations.Queries.GetClaimableOrganisations;
using Aethon.Application.Organisations.Queries.GetMyClaimRequests;
using Aethon.Application.Organisations.Queries.GetMyMemberProfile;
using Aethon.Application.Organisations.Queries.GetMyOrganisationProfile;
using Aethon.Application.Organisations.Queries.GetOrganisationDomains;
using Aethon.Application.Organisations.Queries.GetOrganisationMemberDetail;
using Aethon.Application.Organisations.Queries.GetOrganisationMembers;
using Aethon.Application.Organisations.Queries.GetPublicOrganisationProfile;
using Aethon.Application.Organisations.Services;
using Aethon.Application.RecruiterCompanies.Commands.CreateRecruiterCompanyRequest;
using Aethon.Application.RecruiterCompanies.Queries.GetRecruiterCompanies;
using Aethon.Application.RecruiterCompanies.Commands.CancelRecruiterCompanyRequest;
using Aethon.Application.CompanyRecruiters.Queries.GetCompanyRecruiters;
using Aethon.Application.CompanyRecruiters.Queries.GetPendingCompanyRecruiters;
using Aethon.Application.CompanyRecruiters.Commands.CreateCompanyRecruiterInvite;
using Aethon.Application.CompanyRecruiters.Commands.ApproveCompanyRecruiter;
using Aethon.Application.CompanyRecruiters.Commands.RejectCompanyRecruiter;
using Aethon.Application.CompanyRecruiters.Commands.SuspendCompanyRecruiter;
using Aethon.Application.RecruiterJobs.Commands.CreateRecruiterJobDraft;
using Aethon.Application.RecruiterJobs.Queries.GetRecruiterJobs;
using Aethon.Application.RecruiterJobs.Commands.UpdateRecruiterJobDraft;
using Aethon.Application.RecruiterJobs.Commands.SubmitRecruiterJobForApproval;
using Aethon.Application.CompanyJobs.Queries.GetPendingJobApprovals;
using Aethon.Application.CompanyJobs.Commands.ApproveRecruiterJob;
using Aethon.Application.CompanyJobs.Commands.RejectRecruiterJob;
using Aethon.Application.Verification.Commands.SubmitVerificationRequest;
using Aethon.Application.Verification.Queries.GetMyVerificationRequest;
using Microsoft.Extensions.DependencyInjection;

namespace Aethon.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<OrganisationAccessService>();

        services.AddScoped<ActivityLogWriter>();
        services.AddScoped<ApplicationAccessService>();
        services.AddScoped<ApplicationWorkflowService>();

        services.AddScoped<IWebhookEventDispatcher, WebhookEventDispatcher>();

        services.AddScoped<CreateJobHandler>();
        services.AddScoped<UpdateJobHandler>();
        services.AddScoped<PublishJobHandler>();
        services.AddScoped<CloseJobHandler>();
        services.AddScoped<ReturnJobToDraftHandler>();
        services.AddScoped<PutJobOnHoldHandler>();
        services.AddScoped<GetJobByIdHandler>();
        services.AddScoped<GetMyOrgJobsHandler>();
        services.AddScoped<GetPublicJobsHandler>();
        services.AddScoped<GetPublicJobDetailHandler>();
        services.AddScoped<GetPublicJobLocationsHandler>();
        services.AddScoped<EmailJobApplicationHandler>();

        services.AddScoped<GetMyOrganisationProfileHandler>();
        services.AddScoped<UpdateMyOrganisationProfileHandler>();
        services.AddScoped<GetOrganisationMembersHandler>();
        services.AddScoped<GetOrganisationMemberDetailHandler>();
        services.AddScoped<UpdateMemberRoleHandler>();
        services.AddScoped<UpdateMemberStatusHandler>();
        services.AddScoped<VerifyMemberIdentityHandler>();
        services.AddScoped<RemoveOrganisationMemberHandler>();
        services.AddScoped<GetMyMemberProfileHandler>();
        services.AddScoped<UpsertMyMemberProfileHandler>();
        services.AddScoped<UpdateMyDisplayNameHandler>();
        services.AddScoped<CreateOrganisationInviteHandler>();
        services.AddScoped<AcceptOrganisationInviteHandler>();
        services.AddScoped<GetOrganisationDomainsHandler>();
        services.AddScoped<AddOrganisationDomainHandler>();
        services.AddScoped<ConfirmDomainVerificationHandler>();
        services.AddScoped<RegenerateDomainVerificationTokenHandler>();
        services.AddScoped<GetClaimableOrganisationsHandler>();
        services.AddScoped<SubmitOrganisationClaimHandler>();
        services.AddScoped<GetMyClaimRequestsHandler>();
        services.AddScoped<CancelClaimRequestHandler>();
        services.AddScoped<GetPublicOrganisationProfileHandler>();

        services.AddScoped<SubmitJobApplicationHandler>();
        services.AddScoped<GetApplicationByIdHandler>();
        services.AddScoped<GetMyApplicationsHandler>();
        services.AddScoped<ChangeApplicationStatusHandler>();
        services.AddScoped<AddApplicationNoteHandler>();
        services.AddScoped<AddApplicationCommentHandler>();
        services.AddScoped<ScheduleInterviewHandler>();
        services.AddScoped<GetApplicationsForJobHandler>();
        services.AddScoped<GetApplicationTimelineHandler>();
        services.AddScoped<AttachApplicationFileHandler>();
        services.AddScoped<GetApplicationFilesHandler>();

        services.AddScoped<GetMyCandidateProfileHandler>();
        services.AddScoped<GetPublicJobSeekerProfileHandler>();
        services.AddScoped<UpsertMyCandidateProfileHandler>();
        services.AddScoped<AddCandidateResumeHandler>();
        services.AddScoped<SetDefaultCandidateResumeHandler>();
        services.AddScoped<RemoveCandidateResumeHandler>();
        services.AddScoped<GetResumeAnalysisHandler>();
        services.AddScoped<TriggerResumeAnalysisHandler>();

        services.AddScoped<UploadStoredFileHandler>();

        services.AddScoped<CreateWebhookSubscriptionHandler>();
        services.AddScoped<GetWebhookSubscriptionsHandler>();

        services.AddScoped<CreateRecruiterCompanyRequestHandler>();
        services.AddScoped<GetRecruiterCompaniesHandler>();
        services.AddScoped<CancelRecruiterCompanyRequestHandler>();

        services.AddScoped<GetCompanyRecruitersHandler>();
        services.AddScoped<GetPendingCompanyRecruitersHandler>();
        services.AddScoped<CreateCompanyRecruiterInviteHandler>();
        services.AddScoped<ApproveCompanyRecruiterHandler>();
        services.AddScoped<RejectCompanyRecruiterHandler>();
        services.AddScoped<SuspendCompanyRecruiterHandler>();

        services.AddScoped<CreateRecruiterJobDraftHandler>();
        services.AddScoped<GetRecruiterJobsHandler>();
        services.AddScoped<UpdateRecruiterJobDraftHandler>();
        services.AddScoped<SubmitRecruiterJobForApprovalHandler>();

        services.AddScoped<GetPendingJobApprovalsHandler>();
        services.AddScoped<ApproveRecruiterJobHandler>();
        services.AddScoped<RejectRecruiterJobHandler>();

        services.AddScoped<SubmitVerificationRequestHandler>();
        services.AddScoped<GetMyVerificationRequestHandler>();

        return services;
    }
}
