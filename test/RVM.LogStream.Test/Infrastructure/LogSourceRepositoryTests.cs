using Microsoft.EntityFrameworkCore;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Infrastructure.Data;
using RVM.LogStream.Infrastructure.Repositories;

namespace RVM.LogStream.Test.Infrastructure;

public class LogSourceRepositoryTests : IDisposable
{
    private readonly LogStreamDbContext _db;
    private readonly LogSourceRepository _repo;

    public LogSourceRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LogStreamDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _db = new LogStreamDbContext(options);
        _repo = new LogSourceRepository(_db);
    }

    public void Dispose() => _db.Dispose();

    [Fact]
    public async Task AddAsync_PersistsSource()
    {
        var source = new LogSource { Name = "my-api", TotalCount = 10 };
        await _repo.AddAsync(source);

        Assert.Equal(1, await _db.LogSources.CountAsync());
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsCorrectSource()
    {
        _db.LogSources.Add(new LogSource { Name = "api-a" });
        _db.LogSources.Add(new LogSource { Name = "api-b" });
        await _db.SaveChangesAsync();

        var found = await _repo.GetByNameAsync("api-a");

        Assert.NotNull(found);
        Assert.Equal("api-a", found.Name);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsNullWhenNotFound()
    {
        var found = await _repo.GetByNameAsync("nonexistent");
        Assert.Null(found);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllOrderedByName()
    {
        _db.LogSources.Add(new LogSource { Name = "zebra" });
        _db.LogSources.Add(new LogSource { Name = "alpha" });
        await _db.SaveChangesAsync();

        var all = await _repo.GetAllAsync();

        Assert.Equal(2, all.Count);
        Assert.Equal("alpha", all[0].Name);
        Assert.Equal("zebra", all[1].Name);
    }

    [Fact]
    public async Task UpdateAsync_ModifiesSource()
    {
        var source = new LogSource { Name = "my-api", TotalCount = 5 };
        _db.LogSources.Add(source);
        await _db.SaveChangesAsync();

        source.TotalCount = 20;
        source.LastSeen = DateTime.UtcNow;
        await _repo.UpdateAsync(source);

        var updated = await _db.LogSources.FirstAsync();
        Assert.Equal(20, updated.TotalCount);
    }
}
