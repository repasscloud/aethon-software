using System.ComponentModel.DataAnnotations;
using Aethon.Shared.Enums;

namespace Aethon.Shared.Organisations;

public sealed class AddOrganisationDomainRequestDto
{
    [Required]
    [MaxLength(253)]
    public string Domain { get; set; } = string.Empty;

    public DomainVerificationMethod VerificationMethod { get; set; } = DomainVerificationMethod.DnsTxt;
}
