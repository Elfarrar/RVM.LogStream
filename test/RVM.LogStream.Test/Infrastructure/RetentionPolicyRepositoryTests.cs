using Microsoft.EntityFrameworkCore;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Infrastructure.Data;
using RVM.LogStream.Infrastructure.Repositories;

namespace RVM.LogStream.Test.Infrastructure;

public class RetentionPolicyRepositoryTests : IDisposable
{
    private readonly LogStreamDbContext _db;
    private readonly RetentionPolicyRepository _repo;

    public RetentionPolicyRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LogStreamDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new LogStreamDbContext(options);
        _repo = new RetentionPolicyRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task AddAsync_PersistsPolicy()
    {
        var policy = new RetentionPolicy { SourcePattern = "my-api", RetentionDays = 14 };
        await _repo.AddAsync(policy);

        Assert.Equal(1, await _db.RetentionPolicies.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsCorrectPolicy()
    {
        var policy = new RetentionPolicy { SourcePattern = "test" };
        _db.RetentionPolicies.Add(policy);
        await _db.SaveChangesAsync();

        var found = await _repo.GetByIdAsync(policy.Id);

        Assert.NotNull(found);
        Assert.Equal("test", found.SourcePattern);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNullWhenNotFound()
    {
        var found = await _repo.GetByIdAsync(Guid.NewGuid());
        Assert.Null(found);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrderedByPattern()
    {
        _db.RetentionPolicies.Add(new RetentionPolicy { SourcePattern = "zebra" });
        _db.RetentionPolicies.Add(new RetentionPolicy { SourcePattern = "alpha" });
        await _db.SaveChangesAsync();

        var all = await _repo.GetAllAsync();

        Assert.Equal(2, all.Count);
        Assert.Equal("alpha", all[0].SourcePattern);
        Assert.Equal("zebra", all[1].SourcePattern);
    }

    [Fact]
    public async Task GetEnabledAsync_ReturnsOnlyEnabled()
    {
        _db.RetentionPolicies.Add(new RetentionPolicy { SourcePattern = "enabled", IsEnabled = true });
        _db.RetentionPolicies.Add(new RetentionPolicy { SourcePattern = "disabled", IsEnabled = false });
        await _db.SaveChangesAsync();

        var enabled = await _repo.GetEnabledAsync();

        Assert.Single(enabled);
        Assert.Equal("enabled", enabled[0].SourcePattern);
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAt()
    {
        var policy = new RetentionPolicy { SourcePattern = "test", RetentionDays = 30 };
        _db.RetentionPolicies.Add(policy);
        await _db.SaveChangesAsync();
        Assert.Null(policy.UpdatedAt);

        policy.RetentionDays = 7;
        await _repo.UpdateAsync(policy);

        var updated = await _db.RetentionPolicies.FirstAsync();
        Assert.Equal(7, updated.RetentionDays);
        Assert.NotNull(updated.UpdatedAt);
    }

    [Fact]
    public async Task DeleteAsync_RemovesPolicy()
    {
        var policy = new RetentionPolicy { SourcePattern = "delete-me" };
        _db.RetentionPolicies.Add(policy);
        await _db.SaveChangesAsync();

        await _repo.DeleteAsync(policy.Id);

        Assert.Equal(0, await _db.RetentionPolicies.CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_DoesNothingWhenNotFound()
    {
        await _repo.DeleteAsync(Guid.NewGuid());
        Assert.Equal(0, await _db.RetentionPolicies.CountAsync());
    }
}
