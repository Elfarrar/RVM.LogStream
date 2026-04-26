using Moq;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.API.Services;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Enums;
using RVM.LogStream.Domain.Interfaces;
using RVM.LogStream.Domain.Models;

namespace RVM.LogStream.Test.Services;

public class LogSearchServiceTests
{
    private readonly Mock<ILogEntryRepository> _repo = new();
    private readonly LogSearchService _service;

    public LogSearchServiceTests()
    {
        _service = new LogSearchService(_repo.Object);
    }

    private static LogEntry MakeEntry(string message = "msg", string source = "api", LogLevel level = LogLevel.Information)
        => new()
        {
            Id = Guid.NewGuid(),
            Timestamp = DateTime.UtcNow,
            Level = level,
            Message = message,
            Source = source,
        };

    [Fact]
    public async Task SearchAsync_ReturnsPagedResult()
    {
        var entries = new List<LogEntry> { MakeEntry("hello"), MakeEntry("world") };
        _repo.Setup(r => r.SearchAsync(null, null, null, null, null, null, 0, 20, It.IsAny<CancellationToken>()))
             .ReturnsAsync(entries);
        _repo.Setup(r => r.CountAsync(null, null, null, null, null, null, It.IsAny<CancellationToken>()))
             .ReturnsAsync(2);

        var result = await _service.SearchAsync(null, null, null, null, null, null, 0, 20);

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(0, result.Offset);
        Assert.Equal(20, result.Limit);
    }

    [Fact]
    public async Task SearchAsync_ParsesKnownLevel()
    {
        _repo.Setup(r => r.SearchAsync(null, null, LogLevel.Error, null, null, null, 0, 50, It.IsAny<CancellationToken>()))
             .ReturnsAsync([MakeEntry(level: LogLevel.Error)]);
        _repo.Setup(r => r.CountAsync(null, null, LogLevel.Error, null, null, null, It.IsAny<CancellationToken>()))
             .ReturnsAsync(1);

        var result = await _service.SearchAsync(null, null, "Error", null, null, null, 0, 50);

        Assert.Single(result.Items);
        Assert.Equal("Error", result.Items[0].Level);
    }

    [Fact]
    public async Task SearchAsync_UnknownLevelPassesNullToRepo()
    {
        _repo.Setup(r => r.SearchAsync(null, null, null, null, null, null, 0, 50, It.IsAny<CancellationToken>()))
             .ReturnsAsync([]);
        _repo.Setup(r => r.CountAsync(null, null, null, null, null, null, It.IsAny<CancellationToken>()))
             .ReturnsAsync(0);

        var result = await _service.SearchAsync(null, null, "NotALevel", null, null, null, 0, 50);

        Assert.Empty(result.Items);
        _repo.Verify(r => r.SearchAsync(null, null, null, null, null, null, 0, 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_EmptyLevelPassesNullToRepo()
    {
        _repo.Setup(r => r.SearchAsync(null, null, null, null, null, null, 0, 10, It.IsAny<CancellationToken>()))
             .ReturnsAsync([]);
        _repo.Setup(r => r.CountAsync(null, null, null, null, null, null, It.IsAny<CancellationToken>()))
             .ReturnsAsync(0);

        await _service.SearchAsync(null, null, "", null, null, null, 0, 10);

        _repo.Verify(r => r.SearchAsync(null, null, null, null, null, null, 0, 10, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SearchAsync_MapsEntriesToResponse()
    {
        var entry = MakeEntry("my-message", "my-source", LogLevel.Warning);
        entry.CorrelationId = "corr-123";
        entry.Properties = "{\"k\":1}";

        _repo.Setup(r => r.SearchAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<LogLevel?>(),
                    It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync([entry]);
        _repo.Setup(r => r.CountAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<LogLevel?>(),
                    It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
             .ReturnsAsync(1);

        var result = await _service.SearchAsync("my", "my-source", "Warning", "corr-123", null, null, 0, 10);

        var item = result.Items[0];
        Assert.Equal(entry.Id, item.Id);
        Assert.Equal("my-message", item.Message);
        Assert.Equal("my-source", item.Source);
        Assert.Equal("Warning", item.Level);
        Assert.Equal("corr-123", item.CorrelationId);
        Assert.Equal("{\"k\":1}", item.Properties);
    }

    [Fact]
    public async Task SearchAsync_ReturnsZeroWhenNoResults()
    {
        _repo.Setup(r => r.SearchAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<LogLevel?>(),
                    It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                    It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync([]);
        _repo.Setup(r => r.CountAsync(It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<LogLevel?>(),
                    It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
                    It.IsAny<CancellationToken>()))
             .ReturnsAsync(0);

        var result = await _service.SearchAsync("noresult", null, null, null, null, null, 0, 50);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
