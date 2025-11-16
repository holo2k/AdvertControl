using AdControl.Domain.Models;

namespace AdControl.Application.Repository.Abstractions;

public interface IScreenRepository
{
    Task<Screen?> GetAsync(Guid id, CancellationToken ct = default);
    Task<List<Screen>> ListAsync(string? filterName, int limit, int offset, CancellationToken ct = default);
    Task<Screen> CreateAsync(Screen screen, CancellationToken ct = default);
    Task UpdateAsync(Screen screen, CancellationToken ct = default);
    Task UpdateLastHeartBeatAsync(Guid screenId, CancellationToken ct = default);
    Task<IQueryable<Screen>> GetListByUserIdAsync(Guid userId, CancellationToken ct = default);
}