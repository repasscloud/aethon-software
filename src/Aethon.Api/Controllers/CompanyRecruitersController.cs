using Aethon.Api.Common;
using Aethon.Application.CompanyRecruiters;
using Aethon.Shared.CompanyRecruiters;
using Aethon.Shared.RecruiterCompanies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Controllers;

[ApiController]
[Route("api/company/recruiters")]
[Authorize]
public sealed class CompanyRecruitersController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecruiterCompanyRelationshipDto>>> GetMine(
        [FromServices] ICompanyRecruiterQueryService service,
        CancellationToken cancellationToken)
    {
        var companyUserId = User.GetUserId();
        var result = await service.GetRelationshipsAsync(companyUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<RecruiterCompanyRelationshipDto>>> GetPending(
        [FromServices] ICompanyRecruiterQueryService service,
        CancellationToken cancellationToken)
    {
        var companyUserId = User.GetUserId();
        var result = await service.GetPendingRequestsAsync(companyUserId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("invites")]
    public async Task<IActionResult> Invite(
        [FromBody] CreateCompanyRecruiterInviteDto request,
        [FromServices] ICompanyRecruiterCommandService service,
        CancellationToken cancellationToken)
    {
        var companyUserId = User.GetUserId();
        await service.InviteAsync(companyUserId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{relationshipId:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid relationshipId,
        [FromBody] ApproveRecruiterCompanyRequestDto request,
        [FromServices] ICompanyRecruiterCommandService service,
        CancellationToken cancellationToken)
    {
        var companyUserId = User.GetUserId();
        await service.ApproveAsync(companyUserId, relationshipId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{relationshipId:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid relationshipId,
        [FromBody] RejectRecruiterCompanyRequestDto request,
        [FromServices] ICompanyRecruiterCommandService service,
        CancellationToken cancellationToken)
    {
        var companyUserId = User.GetUserId();
        await service.RejectAsync(companyUserId, relationshipId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{relationshipId:guid}/suspend")]
    public async Task<IActionResult> Suspend(
        Guid relationshipId,
        [FromServices] ICompanyRecruiterCommandService service,
        CancellationToken cancellationToken)
    {
        var companyUserId = User.GetUserId();
        await service.SuspendAsync(companyUserId, relationshipId, cancellationToken);
        return NoContent();
    }
}
