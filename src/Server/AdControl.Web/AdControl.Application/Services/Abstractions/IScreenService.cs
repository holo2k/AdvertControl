using AdControl.Domain.Models;

namespace AdControl.Application.Services.Abstractions;

public interface IScreenService
{
    Task<Screen> CreateAsync(string name, string resolution, string location, Guid? userId = null,
        CancellationToken ct = default);

    Task<Screen?> GetAsync(Guid id, CancellationToken ct = default);

    Task<(List<Screen> Items, int Total)> ListAsync(string? filter, int limit, int offset,
        CancellationToken ct = default);

    Task UpdateLastHeartBeatAsync(Guid screenId, CancellationToken ct = default);
    
    Task<IQueryable<Screen>> GetListByUserIdAsync(Guid userId, CancellationToken ct = default);
}