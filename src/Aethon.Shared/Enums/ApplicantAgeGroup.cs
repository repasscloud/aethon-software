namespace Aethon.Shared.Enums;

/// <summary>
/// Age classification for a job seeker.
///
/// Used to enforce age-appropriate job visibility and application eligibility
/// in compliance with APPs (Australia), GDPR (EU), and CCPA/CPRA (California).
///
/// We do not store date of birth. School leavers provide birth month/year only.
/// Adults confirm they are 18+ — no date captured.
/// </summary>
public enum ApplicantAgeGroup
{
    /// <summary>
    /// Age group not yet confirmed. The job seeker must set this before applying.
    /// </summary>
    NotSpecified = 0,

    /// <summary>
    /// School leaver — aged 16–18.
    /// Birth month and year are stored on the profile for account lifecycle management.
    /// No full date of birth is captured.
    /// </summary>
    SchoolLeaver = 1,

    /// <summary>
    /// Adult — confirmed 18 or older.
    /// No date of birth or birth month/year is captured.
    /// </summary>
    Adult = 2
}
