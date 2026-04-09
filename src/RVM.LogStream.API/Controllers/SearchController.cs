using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.API.Services;

namespace RVM.LogStream.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SearchController(LogSearchService searchService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SearchLogsResponse>> Search(
        [FromQuery] string? query,
        [FromQuery] string? source,
        [FromQuery] string? level,
        [FromQuery] string? correlationId,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 200);
        offset = Math.Max(0, offset);

        var result = await searchService.SearchAsync(query, source, level, correlationId, from, to, offset, limit, ct);
        return Ok(result);
    }
}
