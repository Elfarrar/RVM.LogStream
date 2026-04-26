using Microsoft.AspNetCore.Mvc;
using Moq;
using RVM.LogStream.API.Controllers;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Interfaces;

namespace RVM.LogStream.Test.Controllers;

public class SourcesControllerTests
{
    private readonly Mock<ILogSourceRepository> _sourceRepo = new();
    private readonly SourcesController _controller;

    public SourcesControllerTests()
    {
        _controller = new SourcesController(_sourceRepo.Object);
    }

    private static LogSource MakeSource(string name = "api", long total = 10)
        => new() { Name = name, TotalCount = total };

    [Fact]
    public async Task GetAll_ReturnsAllSources()
    {
        var sources = new List<LogSource> { MakeSource("api"), MakeSource("worker", 50) };
        _sourceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(sources);

        var result = await _controller.GetAll(CancellationToken.None);

        // implicit ActionResult<T> sets Value, not Result
        var list = Assert.IsType<List<LogSourceResponse>>(result.Value);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetAll_EmptyRepo_ReturnsEmptyList()
    {
        _sourceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await _controller.GetAll(CancellationToken.None);

        var list = Assert.IsType<List<LogSourceResponse>>(result.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetAll_MapsSourceFields()
    {
        var source = MakeSource("my-api", 999);
        _sourceRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([source]);

        var result = await _controller.GetAll(CancellationToken.None);

        var list = Assert.IsType<List<LogSourceResponse>>(result.Value);
        Assert.Equal("my-api", list[0].Name);
        Assert.Equal(999, list[0].TotalCount);
        Assert.Equal(source.Id, list[0].Id);
    }

    [Fact]
    public async Task GetByName_ExistingSource_ReturnsOk()
    {
        var source = MakeSource("api-service", 42);
        _sourceRepo.Setup(r => r.GetByNameAsync("api-service", It.IsAny<CancellationToken>())).ReturnsAsync(source);

        var result = await _controller.GetByName("api-service", CancellationToken.None);

        var response = Assert.IsType<LogSourceResponse>(result.Value);
        Assert.Equal("api-service", response.Name);
        Assert.Equal(42, response.TotalCount);
    }

    [Fact]
    public async Task GetByName_NotFound_Returns404()
    {
        _sourceRepo.Setup(r => r.GetByNameAsync("unknown", It.IsAny<CancellationToken>()))
                   .ReturnsAsync((LogSource?)null);

        var result = await _controller.GetByName("unknown", CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task GetByName_CallsRepoWithCorrectName()
    {
        _sourceRepo.Setup(r => r.GetByNameAsync("specific-name", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(MakeSource("specific-name"));

        await _controller.GetByName("specific-name", CancellationToken.None);

        _sourceRepo.Verify(r => r.GetByNameAsync("specific-name", It.IsAny<CancellationToken>()), Times.Once);
    }
}
