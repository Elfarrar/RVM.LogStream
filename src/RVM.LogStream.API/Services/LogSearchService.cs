using RVM.LogStream.API.Dtos;
using RVM.LogStream.Domain.Interfaces;

using LogLevel = RVM.LogStream.Domain.Enums.LogLevel;

namespace RVM.LogStream.API.Services;

public class LogSearchService(ILogEntryRepository logEntryRepo)
{
    public async Task<SearchLogsResponse> SearchAsync(
        string? query, string? source, string? level, string? correlationId,
        DateTime? from, DateTime? to, int offset, int limit,
        CancellationToken ct = default)
    {
        LogLevel? parsedLevel = null;
        if (!string.IsNullOrEmpty(level) && Enum.TryParse<LogLevel>(level, true, out var lv))
            parsedLevel = lv;

        var entries = await logEntryRepo.SearchAsync(query, source, parsedLevel, correlationId, from, to, offset, limit, ct);
        var totalCount = await logEntryRepo.CountAsync(query, source, parsedLevel, correlationId, from, to, ct);

        var items = entries.Select(e => new LogEntryResponse(
            e.Id, e.Timestamp, e.Level.ToString(), e.Message, e.MessageTemplate,
            e.Source, e.CorrelationId, e.Properties, e.Exception)).ToList();

        return new SearchLogsResponse(items, totalCount, offset, limit);
    }
}
