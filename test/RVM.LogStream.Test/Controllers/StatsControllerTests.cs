using Microsoft.AspNetCore.Mvc;
using Moq;
using RVM.LogStream.API.Controllers;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Enums;
using RVM.LogStream.Domain.Interfaces;
using RVM.LogStream.Domain.Models;

namespace RVM.LogStream.Test.Controllers;

public class StatsControllerTests
{
    private readonly Mock<ILogSourceRepository> _sourceRepo = new();
    private readonly Mock<ILogEntryRepository> _logEntryRepo = new();
    private readonly StatsController _controller;

    public StatsControllerTests()
    {
        _controller = new StatsController(_sourceRepo.Object, _logEntryRepo.Object);
    }

    [Fact]
    public async Task GetStats_ReturnsOkResult()
    {
        _sourceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _logEntryRepo.Setup(r => r.GetVolumeByLevelAsync(It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync([]);
        _logEntryRepo.Setup(r => r.GetVolumeBySourceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync([]);

        var result = await _controller.GetStats(null, null, null, CancellationToken.None);

        // implicit ActionResult<T>: value is in .Value
        Assert.NotNull(result.Value);
    }

    [Fact]
    public async Task GetStats_SumsTotalCountFromSources()
    {
        var sources = new List<LogSource>
        {
            new() { Name = "api", TotalCount = 100 },
            new() { Name = "worker", TotalCount = 250 },
        };
        _sourceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sources);
        _logEntryRepo.Setup(r => r.GetVolumeByLevelAsync(It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync([]);
        _logEntryRepo.Setup(r => r.GetVolumeBySourceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync([]);

        var result = await _controller.GetStats(null, null, null, CancellationToken.None);

        var stats = Assert.IsType<LogStatsResponse>(result.Value);
        Assert.Equal(350, stats.TotalLogs);
        Assert.Equal(2, stats.SourceCount);
    }

    [Fact]
    public async Task GetStats_MapsByLevelVolume()
    {
        _sourceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _logEntryRepo.Setup(r => r.GetVolumeByLevelAsync(It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync([
                         new LogVolumeByLevel(LogLevel.Error, 5),
                         new LogVolumeByLevel(LogLevel.Information, 100),
                     ]);
        _logEntryRepo.Setup(r => r.GetVolumeBySourceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync([]);

        var result = await _controller.GetStats(null, null, null, CancellationToken.None);

        var stats = Assert.IsType<LogStatsResponse>(result.Value);
        Assert.Equal(2, stats.ByLevel.Count);
        Assert.Contains(stats.ByLevel, l => l.Level == "Error" && l.Count == 5);
        Assert.Contains(stats.ByLevel, l => l.Level == "Information" && l.Count == 100);
    }

    [Fact]
    public async Task GetStats_MapsBySourceVolume()
    {
        _sourceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _logEntryRepo.Setup(r => r.GetVolumeByLevelAsync(It.IsAny<string?>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync([]);
        _logEntryRepo.Setup(r => r.GetVolumeBySourceAsync(It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync([
                         new LogVolumeBySource("api", 80),
                         new LogVolumeBySource("worker", 20),
                     ]);

        var result = await _controller.GetStats(null, null, null, CancellationToken.None);

        var stats = Assert.IsType<LogStatsResponse>(result.Value);
        Assert.Equal(2, stats.BySource.Count);
        Assert.Contains(stats.BySource, s => s.Source == "api" && s.Count == 80);
    }

    [Fact]
    public async Task GetStats_WithDateRange_PassesRangeToRepo()
    {
        var from = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2025, 1, 31, 23, 59, 59, DateTimeKind.Utc);

        _sourceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);
        _logEntryRepo.Setup(r => r.GetVolumeByLevelAsync(null, from, to, It.IsAny<CancellationToken>()))
                     .ReturnsAsync([]);
        _logEntryRepo.Setup(r => r.GetVolumeBySourceAsync(from, to, It.IsAny<CancellationToken>()))
                     .ReturnsAsync([]);

        var result = await _controller.GetStats(null, from, to, CancellationToken.None);

        Assert.NotNull(result.Value);
        _logEntryRepo.Verify(r => r.GetVolumeByLevelAsync(null, from, to, It.IsAny<CancellationToken>()), Times.Once);
    }
}
