using Aethon.Shared.RecruiterCompanies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Controllers;

[ApiController]
[Route("api/recruiter/companies")]
[Authorize]
public sealed class RecruiterCompaniesController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecruiterCompanyRelationshipDto>>> GetMine(
        [FromServices] IRecruiterCompanyQueryService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await service.GetRecruiterRelationshipsAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("requests")]
    public async Task<ActionResult<RecruiterCompanyRelationshipDto>> CreateRequest(
        [FromBody] CreateRecruiterCompanyRequestDto request,
        [FromServices] IRecruiterCompanyCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await service.CreateRequestAsync(userId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("requests/{relationshipId:guid}/cancel")]
    public async Task<IActionResult> CancelRequest(
        Guid relationshipId,
        [FromServices] IRecruiterCompanyCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await service.CancelRequestAsync(userId, relationshipId, cancellationToken);
        return NoContent();
    }
}
