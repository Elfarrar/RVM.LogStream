using Microsoft.AspNetCore.SignalR;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.API.Hubs;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Interfaces;

using LogLevel = RVM.LogStream.Domain.Enums.LogLevel;

namespace RVM.LogStream.API.Services;

public class LogIngestionService(
    ILogEntryRepository logEntryRepo,
    ILogSourceRepository logSourceRepo,
    IHubContext<LogStreamHub> hubContext,
    ILogger<LogIngestionService> logger)
{
    public async Task<(int Accepted, int Rejected)> IngestAsync(List<IngestLogEntryRequest> batch, CancellationToken ct = default)
    {
        var entries = new List<LogEntry>();
        var rejected = 0;

        foreach (var item in batch)
        {
            if (string.IsNullOrWhiteSpace(item.Message) || string.IsNullOrWhiteSpace(item.Source))
            {
                rejected++;
                continue;
            }

            if (!Enum.TryParse<LogLevel>(item.Level, true, out var level))
                level = LogLevel.Information;

            entries.Add(new LogEntry
            {
                Timestamp = item.Timestamp ?? DateTime.UtcNow,
                Level = level,
                Message = item.Message,
                MessageTemplate = item.MessageTemplate,
                Source = item.Source,
                CorrelationId = item.CorrelationId,
                Properties = item.Properties,
                Exception = item.Exception,
            });
        }

        if (entries.Count > 0)
        {
            await logEntryRepo.AddBatchAsync(entries, ct);

            // Update log sources
            foreach (var group in entries.GroupBy(e => e.Source))
            {
                var source = await logSourceRepo.GetByNameAsync(group.Key, ct);
                if (source is null)
                {
                    source = new LogSource
                    {
                        Name = group.Key,
                        TotalCount = group.Count(),
                    };
                    await logSourceRepo.AddAsync(source, ct);
                }
                else
                {
                    source.LastSeen = DateTime.UtcNow;
                    source.TotalCount += group.Count();
                    await logSourceRepo.UpdateAsync(source, ct);
                }
            }

            // Push to SignalR
            foreach (var entry in entries)
            {
                var dto = new LogEntryResponse(
                    entry.Id, entry.Timestamp, entry.Level.ToString(),
                    entry.Message, entry.MessageTemplate, entry.Source,
                    entry.CorrelationId, entry.Properties, entry.Exception);

                await hubContext.Clients.All.SendAsync("LogReceived", dto, ct);
            }

            logger.LogInformation("Ingested {Count} log entries from {Sources} source(s)",
                entries.Count, entries.Select(e => e.Source).Distinct().Count());
        }

        return (entries.Count, rejected);
    }
}
