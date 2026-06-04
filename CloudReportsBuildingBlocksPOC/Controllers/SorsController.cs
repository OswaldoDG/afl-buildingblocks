namespace CloudReportsBuildingBlocksPOC.Controllers;

using CloudReportsBuildingBlocksPOC.Models.Batches;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
[Consumes("application/json")]
[ApiController]
public partial class SorsController(ILogger<SorsController> logger, IFileBatchService fileBatchService) : ControllerBase
{
    [HttpPost("batch")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(BatchResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<BatchResponseDto>> CreateBatch(BatchRequestDto request)
    {
        LogCreateBatch(request.Items.Count);

        if (request.Items.Count == 0 || string.IsNullOrEmpty(request.DestinationFolder))
        {
            return BadRequest("Batch must contain at least one item and a destination folder");
        }

        var batch = await fileBatchService.Create(request);
        return Ok(batch);
    }

    [HttpPost("batch/complete/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CompleteBatch(BatchCompleteRequestDto request, long id)
    {
        LogCloseBatch(request.Id);

        if (request.Id != id)
        {
            return BadRequest("Id in path and body do not match");
        }

        var batch = await fileBatchService.Complete(request);

        if (batch)
        {
            return Ok(batch);
        }
        else
        {
            return BadRequest("Batch not found or could not be completed");
        }
    }

    [HttpGet("batch/complete/notify/{sessionId}")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> NotifyCompleteBatch(string sessionId)
    {
        await fileBatchService.CompleteSession(sessionId);
        return Ok();
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "SorsController-CreateBatch for {itemCount} items called")]
    partial void LogCreateBatch(int itemCount);

    [LoggerMessage(Level = LogLevel.Debug, Message = "SorsController-CloseBatch for {id} called")]
    partial void LogCloseBatch(long id);
}
