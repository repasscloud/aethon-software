namespace Aethon.Data.Entities;

public sealed class CreditConsumptionLog : EntityBase
{
    public Guid OrganisationJobCreditId { get; set; }
    public OrganisationJobCredit Credit { get; set; } = null!;

    public Guid OrganisationId { get; set; }

    public Guid JobId { get; set; }
    public Job Job { get; set; } = null!;

    public Guid ConsumedByUserId { get; set; }

    /// <summary>
    /// For recruiter-managed jobs where a company user approved publication.
    /// </summary>
    public Guid? ApprovedByUserId { get; set; }

    public int QuantityConsumed { get; set; } = 1;
    public DateTime ConsumedAt { get; set; }
}
