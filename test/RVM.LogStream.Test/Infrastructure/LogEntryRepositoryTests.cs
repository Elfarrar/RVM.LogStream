using Microsoft.EntityFrameworkCore;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Enums;
using RVM.LogStream.Infrastructure.Data;
using RVM.LogStream.Infrastructure.Repositories;

namespace RVM.LogStream.Test.Infrastructure;

public class LogEntryRepositoryTests : IDisposable
{
    private readonly LogStreamDbContext _db;
    private readonly LogEntryRepository _repo;

    public LogEntryRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LogStreamDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new LogStreamDbContext(options);
        _repo = new LogEntryRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task AddBatchAsync_PersistsEntries()
    {
        var entries = new List<LogEntry>
        {
            new() { Message = "msg1", Source = "src1", Level = LogLevel.Information },
            new() { Message = "msg2", Source = "src2", Level = LogLevel.Error },
        };

        await _repo.AddBatchAsync(entries);

        Assert.Equal(2, await _db.LogEntries.CountAsync());
    }

    [Fact]
    public async Task SearchAsync_FiltersBySource()
    {
        await SeedEntries();

        var results = await _repo.SearchAsync(null, "api-a", null, null, null, null, 0, 100);

        Assert.All(results, e => Assert.Equal("api-a", e.Source));
    }

    [Fact]
    public async Task SearchAsync_FiltersByLevel()
    {
        await SeedEntries();

        var results = await _repo.SearchAsync(null, null, LogLevel.Error, null, null, null, 0, 100);

        Assert.All(results, e => Assert.Equal(LogLevel.Error, e.Level));
    }

    [Fact]
    public async Task SearchAsync_FiltersByQuery()
    {
        await SeedEntries();

        var results = await _repo.SearchAsync("critical", null, null, null, null, null, 0, 100);

        Assert.Single(results);
        Assert.Contains("critical", results[0].Message);
    }

    [Fact]
    public async Task SearchAsync_FiltersByCorrelationId()
    {
        await SeedEntries();

        var results = await _repo.SearchAsync(null, null, null, "corr-1", null, null, 0, 100);

        Assert.Single(results);
        Assert.Equal("corr-1", results[0].CorrelationId);
    }

    [Fact]
    public async Task SearchAsync_FiltersByDateRange()
    {
        await SeedEntries();
        var from = DateTime.UtcNow.AddHours(-2);
        var to = DateTime.UtcNow.AddHours(-1);

        var results = await _repo.SearchAsync(null, null, null, null, from, to, 0, 100);

        Assert.All(results, e =>
        {
            Assert.True(e.Timestamp >= from);
            Assert.True(e.Timestamp <= to);
        });
    }

    [Fact]
    public async Task SearchAsync_PaginatesCorrectly()
    {
        await SeedEntries();

        var page1 = await _repo.SearchAsync(null, null, null, null, null, null, 0, 2);
        var page2 = await _repo.SearchAsync(null, null, null, null, null, null, 2, 2);

        Assert.Equal(2, page1.Count);
        Assert.True(page2.Count <= 2);
        Assert.DoesNotContain(page2, e => page1.Any(p => p.Id == e.Id));
    }

    [Fact]
    public async Task CountAsync_ReturnsCorrectTotal()
    {
        await SeedEntries();

        var total = await _repo.CountAsync(null, null, null, null, null, null);
        var filtered = await _repo.CountAsync(null, "api-a", null, null, null, null);

        Assert.Equal(4, total);
        Assert.True(filtered < total);
    }

    [Fact]
    public async Task DeleteOlderThanAsync_RemovesOldEntries()
    {
        var old = new LogEntry
        {
            Message = "old", Source = "src", Level = LogLevel.Information,
            Timestamp = DateTime.UtcNow.AddDays(-60),
        };
        var recent = new LogEntry
        {
            Message = "recent", Source = "src", Level = LogLevel.Information,
            Timestamp = DateTime.UtcNow,
        };
        _db.LogEntries.AddRange(old, recent);
        await _db.SaveChangesAsync();

        var deleted = await _repo.DeleteOlderThanAsync(DateTime.UtcNow.AddDays(-30), null);

        Assert.Equal(1, deleted);
        Assert.Equal(1, await _db.LogEntries.CountAsync());
    }

    [Fact]
    public async Task DeleteOlderThanAsync_FiltersbySourcePattern()
    {
        var old1 = new LogEntry
        {
            Message = "old1", Source = "api-a", Level = LogLevel.Information,
            Timestamp = DateTime.UtcNow.AddDays(-60),
        };
        var old2 = new LogEntry
        {
            Message = "old2", Source = "api-b", Level = LogLevel.Information,
            Timestamp = DateTime.UtcNow.AddDays(-60),
        };
        _db.LogEntries.AddRange(old1, old2);
        await _db.SaveChangesAsync();

        var deleted = await _repo.DeleteOlderThanAsync(DateTime.UtcNow.AddDays(-30), "api-a");

        Assert.Equal(1, deleted);
        var remaining = await _db.LogEntries.SingleAsync();
        Assert.Equal("api-b", remaining.Source);
    }

    [Fact]
    public async Task GetVolumeByLevelAsync_GroupsCorrectly()
    {
        await SeedEntries();
        var from = DateTime.UtcNow.AddHours(-25);
        var to = DateTime.UtcNow.AddHours(1);

        var volumes = await _repo.GetVolumeByLevelAsync(null, from, to);

        Assert.True(volumes.Count >= 2);
        Assert.Contains(volumes, v => v.Level == LogLevel.Information);
        Assert.Contains(volumes, v => v.Level == LogLevel.Error);
    }

    [Fact]
    public async Task GetVolumeBySourceAsync_GroupsCorrectly()
    {
        await SeedEntries();
        var from = DateTime.UtcNow.AddHours(-25);
        var to = DateTime.UtcNow.AddHours(1);

        var volumes = await _repo.GetVolumeBySourceAsync(from, to);

        Assert.True(volumes.Count >= 2);
        Assert.Contains(volumes, v => v.Source == "api-a");
        Assert.Contains(volumes, v => v.Source == "api-b");
    }

    private async Task SeedEntries()
    {
        var entries = new List<LogEntry>
        {
            new() { Message = "info msg", Source = "api-a", Level = LogLevel.Information, CorrelationId = "corr-1", Timestamp = DateTime.UtcNow },
            new() { Message = "critical error", Source = "api-a", Level = LogLevel.Error, Timestamp = DateTime.UtcNow },
            new() { Message = "debug msg", Source = "api-b", Level = LogLevel.Debug, Timestamp = DateTime.UtcNow },
            new() { Message = "warning msg", Source = "api-b", Level = LogLevel.Warning, Timestamp = DateTime.UtcNow.AddHours(-1.5) },
        };
        _db.LogEntries.AddRange(entries);
        await _db.SaveChangesAsync();
    }
}
