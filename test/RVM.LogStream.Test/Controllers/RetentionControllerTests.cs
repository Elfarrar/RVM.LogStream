using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using RVM.LogStream.API.Controllers;
using RVM.LogStream.API.Dtos;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Interfaces;

namespace RVM.LogStream.Test.Controllers;

public class RetentionControllerTests
{
    private readonly Mock<IRetentionPolicyRepository> _policyRepo = new();
    private readonly Mock<ILogger<RetentionController>> _logger = new();
    private readonly RetentionController _controller;

    public RetentionControllerTests()
    {
        _controller = new RetentionController(_policyRepo.Object, _logger.Object);
    }

    private static RetentionPolicy MakePolicy(string pattern = "api-*", int days = 30, bool enabled = true)
        => new() { SourcePattern = pattern, RetentionDays = days, IsEnabled = enabled };

    [Fact]
    public async Task GetAll_ReturnsAllPolicies()
    {
        var policies = new List<RetentionPolicy> { MakePolicy("api-*"), MakePolicy("db-*", 90) };
        _policyRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(policies);

        var result = await _controller.GetAll(CancellationToken.None);

        // returns implicit ActionResult<T> with Value set (not Result)
        var list = Assert.IsType<List<RetentionPolicyResponse>>(result.Value);
        Assert.Equal(2, list.Count);
    }

    [Fact]
    public async Task GetAll_EmptyList_ReturnsEmptyCollection()
    {
        _policyRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync([]);

        var result = await _controller.GetAll(CancellationToken.None);

        var list = Assert.IsType<List<RetentionPolicyResponse>>(result.Value);
        Assert.Empty(list);
    }

    [Fact]
    public async Task GetById_ExistingPolicy_ReturnsOk()
    {
        var policy = MakePolicy();
        _policyRepo.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policy);

        var result = await _controller.GetById(policy.Id, CancellationToken.None);

        var response = Assert.IsType<RetentionPolicyResponse>(result.Value);
        Assert.Equal(policy.Id, response.Id);
        Assert.Equal("api-*", response.SourcePattern);
    }

    [Fact]
    public async Task GetById_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _policyRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((RetentionPolicy?)null);

        var result = await _controller.GetById(id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreated()
    {
        var request = new CreateRetentionPolicyRequest("logs-*", 60, true);
        _policyRepo.Setup(r => r.AddAsync(It.IsAny<RetentionPolicy>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        var result = await _controller.Create(request, CancellationToken.None);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        var response = Assert.IsType<RetentionPolicyResponse>(created.Value);
        Assert.Equal("logs-*", response.SourcePattern);
        Assert.Equal(60, response.RetentionDays);
        Assert.True(response.IsEnabled);
    }

    [Fact]
    public async Task Create_CallsAddAsync()
    {
        var request = new CreateRetentionPolicyRequest("*", 7, false);
        _policyRepo.Setup(r => r.AddAsync(It.IsAny<RetentionPolicy>(), It.IsAny<CancellationToken>()))
                   .Returns(Task.CompletedTask);

        await _controller.Create(request, CancellationToken.None);

        _policyRepo.Verify(r => r.AddAsync(
            It.Is<RetentionPolicy>(p => p.SourcePattern == "*" && p.RetentionDays == 7 && !p.IsEnabled),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _policyRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((RetentionPolicy?)null);

        var result = await _controller.Update(id, new UpdateRetentionPolicyRequest(null, null, null), CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_ExistingPolicy_UpdatesFields()
    {
        var policy = MakePolicy("old-*", 30, true);
        _policyRepo.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policy);
        _policyRepo.Setup(r => r.UpdateAsync(policy, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.Update(policy.Id,
            new UpdateRetentionPolicyRequest("new-*", 90, false), CancellationToken.None);

        // Update returns MapToResponse(policy) implicitly -> Value is set
        var response = Assert.IsType<RetentionPolicyResponse>(result.Value);
        Assert.Equal("new-*", response.SourcePattern);
        Assert.Equal(90, response.RetentionDays);
        Assert.False(response.IsEnabled);
        _policyRepo.Verify(r => r.UpdateAsync(policy, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Update_NullFields_DoesNotOverwriteExistingValues()
    {
        var policy = MakePolicy("keep-*", 45, true);
        _policyRepo.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policy);
        _policyRepo.Setup(r => r.UpdateAsync(policy, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        await _controller.Update(policy.Id, new UpdateRetentionPolicyRequest(null, null, null), CancellationToken.None);

        Assert.Equal("keep-*", policy.SourcePattern);
        Assert.Equal(45, policy.RetentionDays);
        Assert.True(policy.IsEnabled);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        var id = Guid.NewGuid();
        _policyRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((RetentionPolicy?)null);

        var result = await _controller.Delete(id, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_ExistingPolicy_ReturnsNoContent()
    {
        var policy = MakePolicy();
        _policyRepo.Setup(r => r.GetByIdAsync(policy.Id, It.IsAny<CancellationToken>())).ReturnsAsync(policy);
        _policyRepo.Setup(r => r.DeleteAsync(policy.Id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var result = await _controller.Delete(policy.Id, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
        _policyRepo.Verify(r => r.DeleteAsync(policy.Id, It.IsAny<CancellationToken>()), Times.Once);
    }
}
