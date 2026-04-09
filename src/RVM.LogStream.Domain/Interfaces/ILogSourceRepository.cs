using RVM.LogStream.Domain.Entities;

namespace RVM.LogStream.Domain.Interfaces;

public interface ILogSourceRepository
{
    Task<LogSource?> GetByNameAsync(string name, CancellationToken ct = default);
    Task<List<LogSource>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(LogSource source, CancellationToken ct = default);
    Task UpdateAsync(LogSource source, CancellationToken ct = default);
}
