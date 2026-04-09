using Microsoft.EntityFrameworkCore;
using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Interfaces;
using RVM.LogStream.Infrastructure.Data;

namespace RVM.LogStream.Infrastructure.Repositories;

public class RetentionPolicyRepository(LogStreamDbContext db) : IRetentionPolicyRepository
{
    public Task<RetentionPolicy?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => db.RetentionPolicies.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<List<RetentionPolicy>> GetAllAsync(CancellationToken ct = default)
        => db.RetentionPolicies.OrderBy(p => p.SourcePattern).ToListAsync(ct);

    public Task<List<RetentionPolicy>> GetEnabledAsync(CancellationToken ct = default)
        => db.RetentionPolicies.Where(p => p.IsEnabled).ToListAsync(ct);

    public async Task AddAsync(RetentionPolicy policy, CancellationToken ct = default)
    {
        db.RetentionPolicies.Add(policy);
        await db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(RetentionPolicy policy, CancellationToken ct = default)
    {
        db.RetentionPolicies.Update(policy);
        await db.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var policy = await db.RetentionPolicies.FindAsync([id], ct);
        if (policy is not null)
        {
            db.RetentionPolicies.Remove(policy);
            await db.SaveChangesAsync(ct);
        }
    }
}
