namespace Aethon.Application.Abstractions.Authentication;

public interface ICurrentUserAccessor
{
    Guid UserId { get; }
    bool IsAuthenticated { get; }
    bool IsStaff { get; }
    /// <summary>Value of the aethon:app_type claim, e.g. "jobseeker", "employer", "recruiter".</summary>
    string? AppType { get; }
}