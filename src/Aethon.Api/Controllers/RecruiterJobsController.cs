using Aethon.Shared.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Aethon.Api.Controllers;

[ApiController]
[Route("api/recruiter/jobs")]
[Authorize]
public sealed class RecruiterJobsController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<JobSummaryDto>>> GetMine(
        [FromServices] IRecruiterJobQueryService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await service.GetRecruiterJobsAsync(userId, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<JobDetailDto>> CreateDraft(
        [FromBody] RecruiterCreateJobDraftDto request,
        [FromServices] IRecruiterJobCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await service.CreateDraftAsync(userId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{jobId:guid}")]
    public async Task<ActionResult<JobDetailDto>> UpdateDraft(
        Guid jobId,
        [FromBody] RecruiterUpdateJobDraftDto request,
        [FromServices] IRecruiterJobCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var result = await service.UpdateDraftAsync(userId, jobId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{jobId:guid}/submit")]
    public async Task<IActionResult> SubmitForApproval(
        Guid jobId,
        [FromServices] IRecruiterJobCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await service.SubmitForApprovalAsync(userId, jobId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{jobId:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(
        Guid jobId,
        [FromServices] IRecruiterJobCommandService service,
        CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        await service.WithdrawAsync(userId, jobId, cancellationToken);
        return NoContent();
    }
}
