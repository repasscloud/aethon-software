namespace Aethon.Application.RecruiterJobs;

public static class RecruiterJobRules
{
    public static void EnsureRecruiterCanManageCompany(
        bool relationshipExists,
        bool relationshipApproved)
    {
        if (!relationshipExists || !relationshipApproved)
        {
            throw new InvalidOperationException("Recruiter is not approved to manage jobs for this company.");
        }
    }

    public static void EnsureDraftEditable(string status)
    {
        if (!string.Equals(status, "Draft", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(status, "PendingCompanyApproval", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Only draft or pending jobs can be edited by recruiter.");
        }
    }
}
