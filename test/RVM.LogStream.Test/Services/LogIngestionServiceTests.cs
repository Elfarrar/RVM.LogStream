using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.API.Hubs;
using RVM.LogStream.API.Services;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Interfaces;

namespace RVM.LogStream.Test.Services;

public class LogIngestionServiceTests
{
    private readonly Mock<ILogEntryRepository> _logEntryRepo = new();
    private readonly Mock<ILogSourceRepository> _logSourceRepo = new();
    private readonly Mock<IHubContext<LogStreamHub>> _hubContext = new();
    private readonly Mock<ILogger<LogIngestionService>> _logger = new();
    private readonly LogIngestionService _service;

    public LogIngestionServiceTests()
    {
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
        _hubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        _logSourceRepo.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LogSource?)null);

        _service = new LogIngestionService(
            _logEntryRepo.Object,
            _logSourceRepo.Object,
            _hubContext.Object,
            _logger.Object);
    }

    [Fact]
    public async Task IngestAsync_AcceptsValidEntries()
    {
        var batch = new List<IngestLogEntryRequest>
        {
            new(DateTime.UtcNow, "Information", "Test message", null, "my-api", null, null, null),
        };

        var (accepted, rejected) = await _service.IngestAsync(batch);

        Assert.Equal(1, accepted);
        Assert.Equal(0, rejected);
        _logEntryRepo.Verify(r => r.AddBatchAsync(It.Is<List<LogEntry>>(l => l.Count == 1), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IngestAsync_RejectsEmptyMessageOrSource()
    {
        var batch = new List<IngestLogEntryRequest>
        {
            new(DateTime.UtcNow, "Information", "", null, "my-api", null, null, null),
            new(DateTime.UtcNow, "Information", "Valid", null, "", null, null, null),
            new(DateTime.UtcNow, "Information", "  ", null, "my-api", null, null, null),
        };

        var (accepted, rejected) = await _service.IngestAsync(batch);

        Assert.Equal(0, accepted);
        Assert.Equal(3, rejected);
        _logEntryRepo.Verify(r => r.AddBatchAsync(It.IsAny<List<LogEntry>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task IngestAsync_DefaultsToInformationForInvalidLevel()
    {
        var batch = new List<IngestLogEntryRequest>
        {
            new(DateTime.UtcNow, "NotALevel", "Test msg", null, "my-api", null, null, null),
        };

        await _service.IngestAsync(batch);

        _logEntryRepo.Verify(r => r.AddBatchAsync(
            It.Is<List<LogEntry>>(l => l[0].Level == RVM.LogStream.Domain.Enums.LogLevel.Information),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IngestAsync_CreatesNewLogSource()
    {
        var batch = new List<IngestLogEntryRequest>
        {
            new(DateTime.UtcNow, "Error", "Test msg", null, "new-source", null, null, null),
        };

        await _service.IngestAsync(batch);

        _logSourceRepo.Verify(r => r.AddAsync(
            It.Is<LogSource>(s => s.Name == "new-source" && s.TotalCount == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IngestAsync_UpdatesExistingLogSource()
    {
        var existingSource = new LogSource { Name = "existing", TotalCount = 10 };
        _logSourceRepo.Setup(r => r.GetByNameAsync("existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSource);

        var batch = new List<IngestLogEntryRequest>
        {
            new(DateTime.UtcNow, "Information", "msg1", null, "existing", null, null, null),
            new(DateTime.UtcNow, "Information", "msg2", null, "existing", null, null, null),
        };

        await _service.IngestAsync(batch);

        Assert.Equal(12, existingSource.TotalCount);
        _logSourceRepo.Verify(r => r.UpdateAsync(existingSource, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IngestAsync_PushesToSignalR()
    {
        var mockClientProxy = new Mock<IClientProxy>();
        var mockClients = new Mock<IHubClients>();
        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
        _hubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        var batch = new List<IngestLogEntryRequest>
        {
            new(DateTime.UtcNow, "Information", "Test", null, "api", null, null, null),
        };

        await _service.IngestAsync(batch);

        mockClientProxy.Verify(c => c.SendCoreAsync("LogReceived",
            It.Is<object?[]>(args => args.Length == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IngestAsync_UsesProvidedTimestamp()
    {
        var ts = new DateTime(2025, 6, 15, 12, 0, 0, DateTimeKind.Utc);
        var batch = new List<IngestLogEntryRequest>
        {
            new(ts, "Information", "Timestamped", null, "api", null, null, null),
        };

        await _service.IngestAsync(batch);

        _logEntryRepo.Verify(r => r.AddBatchAsync(
            It.Is<List<LogEntry>>(l => l[0].Timestamp == ts),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
