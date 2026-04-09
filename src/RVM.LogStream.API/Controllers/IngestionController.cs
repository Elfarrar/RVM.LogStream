using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.API.Services;

namespace RVM.LogStream.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class IngestionController(
    LogIngestionService ingestionService,
    ILogger<IngestionController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<IngestBatchResponse>> Ingest(IngestBatchRequest request, CancellationToken ct)
    {
        if (request.Events is null || request.Events.Count == 0)
            return BadRequest(new { error = "Events list cannot be empty." });

        var (accepted, rejected) = await ingestionService.IngestAsync(request.Events, ct);
        return Ok(new IngestBatchResponse(accepted, rejected));
    }
}
