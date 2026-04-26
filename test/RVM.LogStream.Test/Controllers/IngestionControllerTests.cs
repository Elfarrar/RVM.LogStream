using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Moq;
using RVM.LogStream.API.Controllers;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.API.Hubs;
using RVM.LogStream.API.Services;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Interfaces;

namespace RVM.LogStream.Test.Controllers;

public class IngestionControllerTests
{
    private readonly Mock<ILogEntryRepository> _logEntryRepo = new();
    private readonly Mock<ILogSourceRepository> _logSourceRepo = new();
    private readonly Mock<IHubContext<LogStreamHub>> _hubContext = new();
    private readonly Mock<ILogger<LogIngestionService>> _svcLogger = new();
    private readonly Mock<ILogger<IngestionController>> _ctrlLogger = new();
    private readonly IngestionController _controller;

    public IngestionControllerTests()
    {
        var mockClients = new Mock<IHubClients>();
        var mockClientProxy = new Mock<IClientProxy>();
        mockClients.Setup(c => c.All).Returns(mockClientProxy.Object);
        _hubContext.Setup(h => h.Clients).Returns(mockClients.Object);

        _logSourceRepo.Setup(r => r.GetByNameAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((LogSource?)null);

        var service = new LogIngestionService(
            _logEntryRepo.Object, _logSourceRepo.Object, _hubContext.Object, _svcLogger.Object);

        _controller = new IngestionController(service, _ctrlLogger.Object);
    }

    [Fact]
    public async Task Ingest_NullEvents_ReturnsBadRequest()
    {
        var request = new IngestBatchRequest(null!);

        var result = await _controller.Ingest(request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Ingest_EmptyEvents_ReturnsBadRequest()
    {
        var request = new IngestBatchRequest([]);

        var result = await _controller.Ingest(request, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public async Task Ingest_ValidEvents_ReturnsOkWithCounts()
    {
        var request = new IngestBatchRequest([
            new(DateTime.UtcNow, "Information", "msg1", null, "api", null, null, null),
            new(DateTime.UtcNow, "Information", "msg2", null, "api", null, null, null),
        ]);

        var result = await _controller.Ingest(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<IngestBatchResponse>(ok.Value);
        Assert.Equal(2, response.Accepted);
        Assert.Equal(0, response.Rejected);
    }

    [Fact]
    public async Task Ingest_MixedValidAndInvalid_ReturnsCorrectCounts()
    {
        var request = new IngestBatchRequest([
            new(DateTime.UtcNow, "Information", "good", null, "api", null, null, null),
            new(DateTime.UtcNow, "Information", "", null, "api", null, null, null),   // empty message - rejected
            new(DateTime.UtcNow, "Information", "good2", null, "", null, null, null), // empty source - rejected
        ]);

        var result = await _controller.Ingest(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<IngestBatchResponse>(ok.Value);
        Assert.Equal(1, response.Accepted);
        Assert.Equal(2, response.Rejected);
    }

    [Fact]
    public async Task Ingest_SingleValidEvent_ReturnsOk()
    {
        var request = new IngestBatchRequest([
            new(null, "Error", "An error occurred", null, "my-service", "corr-1", "{}", "exception trace"),
        ]);

        var result = await _controller.Ingest(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<IngestBatchResponse>(ok.Value);
        Assert.Equal(1, response.Accepted);
        Assert.Equal(0, response.Rejected);
    }
}
