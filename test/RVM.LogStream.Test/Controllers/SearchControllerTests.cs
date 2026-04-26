using Microsoft.AspNetCore.Mvc;
using Moq;
using RVM.LogStream.API.Controllers;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.API.Services;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Enums;
using RVM.LogStream.Domain.Interfaces;

namespace RVM.LogStream.Test.Controllers;

public class SearchControllerTests
{
    private readonly Mock<ILogEntryRepository> _logEntryRepo = new();
    private readonly SearchController _controller;

    public SearchControllerTests()
    {
        var service = new LogSearchService(_logEntryRepo.Object);
        _controller = new SearchController(service);
    }

    private void SetupRepo(List<LogEntry> entries, int count)
    {
        _logEntryRepo.Setup(r => r.SearchAsync(
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<LogLevel?>(),
            It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(entries);

        _logEntryRepo.Setup(r => r.CountAsync(
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<LogLevel?>(),
            It.IsAny<string?>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>(),
            It.IsAny<CancellationToken>()))
            .ReturnsAsync(count);
    }

    [Fact]
    public async Task Search_ReturnsOkWithResult()
    {
        SetupRepo([], 0);

        var result = await _controller.Search(null, null, null, null, null, null);

        Assert.IsType<OkObjectResult>(result.Result);
    }

    [Fact]
    public async Task Search_ClampsTooLargeLimit()
    {
        SetupRepo([], 0);

        // limit > 200 should be clamped to 200
        await _controller.Search(null, null, null, null, null, null, limit: 500);

        _logEntryRepo.Verify(r => r.SearchAsync(
            null, null, null, null, null, null, 0, 200, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_ClampsTooSmallLimit()
    {
        SetupRepo([], 0);

        // limit < 1 should be clamped to 1
        await _controller.Search(null, null, null, null, null, null, limit: 0);

        _logEntryRepo.Verify(r => r.SearchAsync(
            null, null, null, null, null, null, 0, 1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_NegativeOffset_NormalisedToZero()
    {
        SetupRepo([], 0);

        await _controller.Search(null, null, null, null, null, null, offset: -5);

        _logEntryRepo.Verify(r => r.SearchAsync(
            null, null, null, null, null, null, 0, 50, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Search_ReturnsMatchingEntries()
    {
        var entry = new LogEntry
        {
            Id = Guid.NewGuid(),
            Message = "hello",
            Source = "svc",
            Level = LogLevel.Information,
            Timestamp = DateTime.UtcNow
        };
        SetupRepo([entry], 1);

        var result = await _controller.Search("hello", "svc", null, null, null, null);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<SearchLogsResponse>(ok.Value);
        Assert.Single(response.Items);
        Assert.Equal("hello", response.Items[0].Message);
    }

    [Fact]
    public async Task Search_DefaultsWorkCorrectly()
    {
        SetupRepo([], 0);

        await _controller.Search(null, null, null, null, null, null);

        // default offset=0, limit=50
        _logEntryRepo.Verify(r => r.SearchAsync(
            null, null, null, null, null, null, 0, 50, It.IsAny<CancellationToken>()), Times.Once);
    }
}
