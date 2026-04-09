using Microsoft.EntityFrameworkCore;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Enums;
using RVM.LogStream.Domain.Interfaces;
using RVM.LogStream.Domain.Models;
using RVM.LogStream.Infrastructure.Data;

namespace RVM.LogStream.Infrastructure.Repositories;

public class LogEntryRepository(LogStreamDbContext db) : ILogEntryRepository
{
    public async Task AddBatchAsync(List<LogEntry> entries, CancellationToken ct = default)
    {
        db.LogEntries.AddRange(entries);
        await db.SaveChangesAsync(ct);
    }

    public async Task<List<LogEntry>> SearchAsync(string? query, string? source, LogLevel? level,
        string? correlationId, DateTime? from, DateTime? to,
        int offset, int limit, CancellationToken ct = default)
    {
        var q = BuildQuery(query, source, level, correlationId, from, to);
        return await q.OrderByDescending(e => e.Timestamp)
            .Skip(offset).Take(limit)
            .ToListAsync(ct);
    }

    public async Task<int> CountAsync(string? query, string? source, LogLevel? level,
        string? correlationId, DateTime? from, DateTime? to,
        CancellationToken ct = default)
    {
        return await BuildQuery(query, source, level, correlationId, from, to).CountAsync(ct);
    }

    public async Task<int> DeleteOlderThanAsync(DateTime cutoff, string? sourcePattern, CancellationToken ct = default)
    {
        var q = db.LogEntries.Where(e => e.Timestamp < cutoff);
        if (!string.IsNullOrEmpty(sourcePattern) && sourcePattern != "*")
            q = q.Where(e => e.Source == sourcePattern);

        var entries = await q.ToListAsync(ct);
        db.LogEntries.RemoveRange(entries);
        await db.SaveChangesAsync(ct);
        return entries.Count;
    }

    public async Task<List<LogVolumeByLevel>> GetVolumeByLevelAsync(string? source, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var q = db.LogEntries.Where(e => e.Timestamp >= from && e.Timestamp <= to);
        if (!string.IsNullOrEmpty(source))
            q = q.Where(e => e.Source == source);

        return await q.GroupBy(e => e.Level)
            .Select(g => new LogVolumeByLevel(g.Key, g.Count()))
            .ToListAsync(ct);
    }

    public async Task<List<LogVolumeBySource>> GetVolumeBySourceAsync(DateTime from, DateTime to, CancellationToken ct = default)
    {
        return await db.LogEntries
            .Where(e => e.Timestamp >= from && e.Timestamp <= to)
            .GroupBy(e => e.Source)
            .Select(g => new LogVolumeBySource(g.Key, g.Count()))
            .ToListAsync(ct);
    }

    private IQueryable<LogEntry> BuildQuery(string? query, string? source, LogLevel? level,
        string? correlationId, DateTime? from, DateTime? to)
    {
        var q = db.LogEntries.AsQueryable();

        if (!string.IsNullOrEmpty(query))
            q = q.Where(e => e.Message.Contains(query));
        if (!string.IsNullOrEmpty(source))
            q = q.Where(e => e.Source == source);
        if (level.HasValue)
            q = q.Where(e => e.Level == level.Value);
        if (!string.IsNullOrEmpty(correlationId))
            q = q.Where(e => e.CorrelationId == correlationId);
        if (from.HasValue)
            q = q.Where(e => e.Timestamp >= from.Value);
        if (to.HasValue)
            q = q.Where(e => e.Timestamp <= to.Value);

        return q;
    }
}
