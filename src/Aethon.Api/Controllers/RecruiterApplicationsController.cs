using Aethon.Api.Common;
using Aethon.Application.RecruiterApplications;
using Aethon.Shared.Applications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Controllers;

[ApiController]
[Route("api/recruiter/applications")]
[Authorize]
public sealed class RecruiterApplicationsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ApplicationSummaryDto>>> GetMine(
        [FromServices] IRecruiterApplicationQueryService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await service.GetAssignedApplicationsAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{applicationId:guid}/assign")]
    public async Task<IActionResult> Assign(
        Guid applicationId,
        [FromBody] AssignApplicationDto request,
        [FromServices] IRecruiterApplicationCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await service.AssignAsync(userId, applicationId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{applicationId:guid}/unassign")]
    public async Task<IActionResult> Unassign(
        Guid applicationId,
        [FromServices] IRecruiterApplicationCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await service.UnassignAsync(userId, applicationId, cancellationToken);
        return NoContent();
    }
}