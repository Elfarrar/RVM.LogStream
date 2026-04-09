using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Enums;
using RVM.LogStream.Domain.Models;

namespace RVM.LogStream.Domain.Interfaces;

public interface ILogEntryRepository
{
    Task AddBatchAsync(List<LogEntry> entries, CancellationToken ct = default);
    Task<List<LogEntry>> SearchAsync(string? query, string? source, LogLevel? level,
        string? correlationId, DateTime? from, DateTime? to,
        int offset, int limit, CancellationToken ct = default);
    Task<int> CountAsync(string? query, string? source, LogLevel? level,
        string? correlationId, DateTime? from, DateTime? to,
        CancellationToken ct = default);
    Task<int> DeleteOlderThanAsync(DateTime cutoff, string? sourcePattern, CancellationToken ct = default);
    Task<List<LogVolumeByLevel>> GetVolumeByLevelAsync(string? source, DateTime from, DateTime to, CancellationToken ct = default);
    Task<List<LogVolumeBySource>> GetVolumeBySourceAsync(DateTime from, DateTime to, CancellationToken ct = default);
}
