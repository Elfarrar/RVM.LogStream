using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.Domain.Interfaces;

namespace RVM.LogStream.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SourcesController(ILogSourceRepository sourceRepo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<LogSourceResponse>>> GetAll(CancellationToken ct)
    {
        var sources = await sourceRepo.GetAllAsync(ct);
        return sources.Select(s => new LogSourceResponse(s.Id, s.Name, s.FirstSeen, s.LastSeen, s.TotalCount)).ToList();
    }

    [HttpGet("{name}")]
    public async Task<ActionResult<LogSourceResponse>> GetByName(string name, CancellationToken ct)
    {
        var source = await sourceRepo.GetByNameAsync(name, ct);
        if (source is null) return NotFound();
        return new LogSourceResponse(source.Id, source.Name, source.FirstSeen, source.LastSeen, source.TotalCount);
    }
}
