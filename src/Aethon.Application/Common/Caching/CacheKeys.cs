namespace Aethon.Application.Common.Caching;

public static class CacheKeys
{
    public static string JobDetail(Guid jobId) => $"jobs:detail:{jobId:N}";

    public static string ApplicationDetail(Guid applicationId) =>
        $"applications:detail:{applicationId:N}";

    public static string CandidateProfile(Guid userId) =>
        $"candidates:profile:{userId:N}";

    public static string MyApplications(Guid userId, int page, int pageSize) =>
        $"applications:mine:{userId:N}:{page}:{pageSize}";

    public static string MyApplicationsPrefix(Guid userId) =>
        $"applications:mine:{userId:N}:";

    public static string JobApplications(Guid jobId, string statusKey, int page, int pageSize) =>
        $"jobs:{jobId:N}:applications:{statusKey}:{page}:{pageSize}";

    public static string JobApplicationsPrefix(Guid jobId) =>
        $"jobs:{jobId:N}:applications:";

    public static string ApplicationTimeline(Guid applicationId) =>
        $"applications:timeline:{applicationId:N}";
}
