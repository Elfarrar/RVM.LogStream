using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.Domain.Interfaces;

namespace RVM.LogStream.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StatsController(
    ILogSourceRepository sourceRepo,
    ILogEntryRepository logEntryRepo) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<LogStatsResponse>> GetStats(
        [FromQuery] string? source,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        CancellationToken ct)
    {
        var toDate = to ?? DateTime.UtcNow;
        var fromDate = from ?? toDate.AddHours(-24);

        var sources = await sourceRepo.GetAllAsync(ct);
        var totalLogs = sources.Sum(s => s.TotalCount);

        var byLevel = await logEntryRepo.GetVolumeByLevelAsync(source, fromDate, toDate, ct);
        var bySource = await logEntryRepo.GetVolumeBySourceAsync(fromDate, toDate, ct);

        return new LogStatsResponse(
            totalLogs,
            sources.Count,
            byLevel.Select(l => new LevelCountResponse(l.Level.ToString(), l.Count)).ToList(),
            bySource.Select(s => new SourceCountResponse(s.Source, s.Count)).ToList());
    }
}
