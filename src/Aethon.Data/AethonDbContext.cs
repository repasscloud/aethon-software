using Aethon.Data.Configurations;
using Aethon.Data.Entities;
using Aethon.Data.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aethon.Data;

public sealed class AethonDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public AethonDbContext(DbContextOptions<AethonDbContext> options)
        : base(options)
    {
    }

    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();
    public DbSet<JobApplicationComment> JobApplicationComments => Set<JobApplicationComment>();
    public DbSet<JobApplicationInterview> JobApplicationInterviews => Set<JobApplicationInterview>();
    public DbSet<JobApplicationInterviewInterviewer> JobApplicationInterviewInterviewers => Set<JobApplicationInterviewInterviewer>();
    public DbSet<JobApplicationNote> JobApplicationNotes => Set<JobApplicationNote>();
    public DbSet<JobApplicationStatusHistory> JobApplicationStatusHistoryEntries => Set<JobApplicationStatusHistory>();

    public DbSet<JobSeekerLanguage> JobSeekerLanguages => Set<JobSeekerLanguage>();
    public DbSet<JobSeekerNationality> JobSeekerNationalities => Set<JobSeekerNationality>();
    public DbSet<JobSeekerProfile> JobSeekerProfiles => Set<JobSeekerProfile>();
    public DbSet<JobSeekerResume> JobSeekerResumes => Set<JobSeekerResume>();
    public DbSet<ResumeAnalysis> ResumeAnalyses => Set<ResumeAnalysis>();

    public DbSet<Organisation> Organisations => Set<Organisation>();
    public DbSet<OrganisationClaimRequest> OrganisationClaimRequests => Set<OrganisationClaimRequest>();
    public DbSet<OrganisationDomain> OrganisationDomains => Set<OrganisationDomain>();
    public DbSet<OrganisationInvitation> OrganisationInvitations => Set<OrganisationInvitation>();
    public DbSet<OrganisationMembership> OrganisationMemberships => Set<OrganisationMembership>();
    public DbSet<OrganisationRecruitmentPartnership> OrganisationRecruitmentPartnerships => Set<OrganisationRecruitmentPartnership>();

    public DbSet<StoredFile> StoredFiles => Set<StoredFile>();
    public DbSet<StripePaymentEvent> StripePaymentEvents => Set<StripePaymentEvent>();

    public DbSet<JobApplicationAttachment> JobApplicationAttachments => Set<JobApplicationAttachment>();
    public DbSet<WebhookSubscription> WebhookSubscriptions => Set<WebhookSubscription>();
    public DbSet<WebhookDelivery> WebhookDeliveries => Set<WebhookDelivery>();

    public DbSet<SystemSetting> SystemSettings => Set<SystemSetting>();
    public DbSet<JobSyndicationRecord> JobSyndicationRecords => Set<JobSyndicationRecord>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AethonDbContext).Assembly);

        builder.ApplyConfiguration(new JobApplicationAttachmentConfiguration());
        builder.ApplyConfiguration(new WebhookSubscriptionConfiguration());
        builder.ApplyConfiguration(new WebhookDeliveryConfiguration());
    }
}