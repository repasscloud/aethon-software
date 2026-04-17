using System.ComponentModel.DataAnnotations;

namespace Aethon.Shared.Organisations;

public sealed class AcceptOrganisationInviteRequestDto
{
    [Required]
    [MaxLength(200)]
    public string Token { get; set; } = "";
}
