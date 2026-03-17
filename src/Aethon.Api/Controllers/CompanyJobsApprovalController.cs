using Aethon.Application.CompanyJobsApproval;
using Aethon.Shared.CompanyJobsApproval;
using Aethon.Shared.Jobs;
using Aethon.Api.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Controllers;

[ApiController]
[Route("api/company/jobs/approvals")]
[Authorize]
public sealed class CompanyJobsApprovalController : ControllerBase
{
    [HttpGet("pending")]
    public async Task<ActionResult<IReadOnlyList<JobSummaryDto>>> GetPending(
        [FromServices] ICompanyJobApprovalQueryService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await service.GetPendingApprovalsAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{jobId:guid}/approve")]
    public async Task<IActionResult> Approve(
        Guid jobId,
        [FromServices] ICompanyJobApprovalCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await service.ApproveAsync(userId, jobId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{jobId:guid}/reject")]
    public async Task<IActionResult> Reject(
        Guid jobId,
        [FromBody] RejectJobApprovalDto request,
        [FromServices] ICompanyJobApprovalCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await service.RejectAsync(userId, jobId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{jobId:guid}/publish")]
    public async Task<IActionResult> Publish(
        Guid jobId,
        [FromServices] ICompanyJobApprovalCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await service.PublishAsync(userId, jobId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{jobId:guid}/hold")]
    public async Task<IActionResult> Hold(
        Guid jobId,
        [FromBody] HoldJobDto request,
        [FromServices] ICompanyJobApprovalCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await service.PutOnHoldAsync(userId, jobId, request, cancellationToken);
        return NoContent();
    }

    [HttpPost("{jobId:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid jobId,
        [FromBody] CancelJobDto request,
        [FromServices] ICompanyJobApprovalCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await service.CancelAsync(userId, jobId, request, cancellationToken);
        return NoContent();
    }
}