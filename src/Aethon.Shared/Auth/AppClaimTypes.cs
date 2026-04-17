namespace Aethon.Shared.Auth;

public static class AppClaimTypes
{
    public const string TenantId = "aethon:tenant_id";
    public const string TenantSlug = "aethon:tenant_slug";
    public const string DisplayName = "aethon:display_name";

    public const string AppType = "aethon:app_type";
    public const string OrganisationId = "aethon:organisation_id";
    public const string OrganisationName = "aethon:organisation_name";
    public const string OrganisationType = "aethon:organisation_type";
    public const string IsOrganisationOwner = "aethon:is_organisation_owner";
    public const string IsSuperAdmin = "aethon:is_super_admin";
    public const string IsAdmin = "aethon:is_admin";
    public const string IsSupport = "aethon:is_support";
    public const string MustChangePassword = "aethon:must_change_password";
    public const string MustEnableMfa = "aethon:must_enable_mfa";
    public const string CompanyRole = "aethon:company_role";
    public const string RecruiterRole = "aethon:recruiter_role";
    public const string HasJobSeekerProfile = "aethon:has_job_seeker_profile";
}
