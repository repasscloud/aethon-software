namespace Aethon.Shared.Jobs;

public sealed class CreateJobDto
{
    public Guid CompanyOrganisationId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Summary { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? Location { get; set; }

    public decimal? SalaryMin { get; set; }

    public decimal? SalaryMax { get; set; }
}
