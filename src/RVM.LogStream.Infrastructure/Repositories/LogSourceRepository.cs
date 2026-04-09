using Microsoft.EntityFrameworkCore;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Interfaces;
using RVM.LogStream.Infrastructure.Data;

namespace RVM.LogStream.Infrastructure.Repositories;

public class LogSourceRepository(LogStreamDbContext db) : ILogSourceRepository
{
    public Task<LogSource?> GetByNameAsync(string name, CancellationToken ct = default)
        => db.LogSources.FirstOrDefaultAsync(s => s.Name == name, ct);

    public Task<List<LogSource>> GetAllAsync(CancellationToken ct = default)
        => db.LogSources.OrderBy(s => s.Name).ToListAsync(ct);

    public async Task AddAsync(LogSource source, CancellationToken ct = default)
    {
        db.LogSources.Add(source);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(LogSource source, CancellationToken ct = default)
    {
        db.LogSources.Update(source);
        await db.SaveChangesAsync(ct);
    }
}
