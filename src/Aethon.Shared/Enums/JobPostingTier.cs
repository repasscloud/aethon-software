namespace Aethon.Shared.Enums;

public enum JobPostingTier
{
    Standard = 1,
    Premium = 2,
    /// <summary>
    /// Assigned automatically to jobs ingested via the external import feed API.
    /// Cannot be selected through the UI or standard job creation flows.
    /// </summary>
    Imported = 3
}
