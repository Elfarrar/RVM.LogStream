using RVM.LogStream.Domain.Entities;

namespace RVM.LogStream.Domain.Interfaces;

public interface IRetentionPolicyRepository
{
    Task<RetentionPolicy?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<List<RetentionPolicy>> GetAllAsync(CancellationToken ct = default);
    Task<List<RetentionPolicy>> GetEnabledAsync(CancellationToken ct = default);
    Task AddAsync(RetentionPolicy policy, CancellationToken ct = default);
    Task UpdateAsync(RetentionPolicy policy, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
