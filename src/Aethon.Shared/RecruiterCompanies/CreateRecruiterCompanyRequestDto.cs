namespace Aethon.Shared.RecruiterCompanies;

public sealed class CreateRecruiterCompanyRequestDto
{
    public Guid CompanyOrganisationId { get; set; }
    public string? Message { get; set; }
}
