namespace CloudReportsBuildingBlocksPOC.Controllers;

using CloudReportsBuildingBlocksPOC.Models.Otdr;
using CloudReportsBuildingBlocksPOC.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[AllowAnonymous]
[ApiController]
public partial class OtdrController(IOtdrService otdrService, ILogger<OtdrController> logger) : ControllerBase
{

    [HttpPost("atd")]
    public async Task<IActionResult> UpsertAtdEntry([FromBody] AtdEntry entry)
    {
        LogCreateEntry();
        await otdrService.UpsertAtd(entry);
        return Ok();
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "OtdrController-UpsertAtdEntry")]
    partial void LogCreateEntry();
}
