namespace Aethon.Shared.Organisations;

public sealed class UpdateMemberRoleRequestDto
{
    /// <summary>Set for company-type organisations. Null to leave unchanged.</summary>
    public string? CompanyRole { get; set; }

    /// <summary>Set for recruiter-type organisations. Null to leave unchanged.</summary>
    public string? RecruiterRole { get; set; }
}
