using Aethon.Application.Activity.Services;
using Aethon.Application.Applications.Commands.AddApplicationComment;
using Aethon.Application.Applications.Commands.AddApplicationNote;
using Aethon.Application.Applications.Commands.ChangeApplicationStatus;
using Aethon.Application.Applications.Commands.ScheduleInterview;
using Aethon.Application.Applications.Commands.SubmitJobApplication;
using Aethon.Application.Applications.Queries.GetApplicationById;
using Aethon.Application.Applications.Queries.GetApplicationTimeline;
using Aethon.Application.Applications.Queries.GetApplicationsForJob;
using Aethon.Application.Applications.Queries.GetMyApplications;
using Aethon.Application.Applications.Services;
using Aethon.Application.Candidates.Commands.AddCandidateResume;
using Aethon.Application.Candidates.Commands.RemoveCandidateResume;
using Aethon.Application.Candidates.Commands.SetDefaultCandidateResume;
using Aethon.Application.Candidates.Commands.UpsertMyCandidateProfile;
using Aethon.Application.Candidates.Queries.GetMyCandidateProfile;
using Aethon.Application.Files.Commands.UploadStoredFile;
using Aethon.Application.Jobs.Commands.CreateJob;
using Aethon.Application.Jobs.Queries.GetJobById;
using Aethon.Application.Organisations.Services;
using Microsoft.Extensions.DependencyInjection;
using Aethon.Application.Applications.Commands.AttachApplicationFile;
using Aethon.Application.Applications.Queries.GetApplicationFiles;

namespace Aethon.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<OrganisationAccessService>();

        services.AddScoped<ActivityLogWriter>();
        services.AddScoped<ApplicationAccessService>();
        services.AddScoped<ApplicationWorkflowService>();

        services.AddScoped<CreateJobHandler>();
        services.AddScoped<GetJobByIdHandler>();

        services.AddScoped<SubmitJobApplicationHandler>();
        services.AddScoped<GetApplicationByIdHandler>();
        services.AddScoped<GetMyApplicationsHandler>();
        services.AddScoped<ChangeApplicationStatusHandler>();
        services.AddScoped<AddApplicationNoteHandler>();
        services.AddScoped<AddApplicationCommentHandler>();
        services.AddScoped<ScheduleInterviewHandler>();
        services.AddScoped<GetApplicationsForJobHandler>();
        services.AddScoped<GetApplicationTimelineHandler>();

        services.AddScoped<GetMyCandidateProfileHandler>();
        services.AddScoped<UpsertMyCandidateProfileHandler>();
        services.AddScoped<AddCandidateResumeHandler>();
        services.AddScoped<SetDefaultCandidateResumeHandler>();
        services.AddScoped<RemoveCandidateResumeHandler>();

        services.AddScoped<UploadStoredFileHandler>();

        services.AddScoped<AttachApplicationFileHandler>();
        services.AddScoped<GetApplicationFilesHandler>();

        return services;
    }
}
