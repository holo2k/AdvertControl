using AdControl.Application.Repository.Abstractions;
using AdControl.Application.Services.Abstractions;
using AdControl.Domain.Models;

namespace AdControl.Application.Services.Implementations;

public class ScreenService : IScreenService
{
    private readonly IScreenRepository _repo;

    public ScreenService(IScreenRepository repo)
    {
        _repo = repo;
    }

    public async Task<Screen> CreateAsync(string name, string resolution, string location, Guid? userId = null,
        CancellationToken ct = default)
    {
        var s = new Screen
        {
            Id = Guid.NewGuid(),
            Name = name,
            Resolution = resolution,
            Location = location,
            UserId = userId,
            PairedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        return await _repo.CreateAsync(s, ct);
    }

    public async Task<Screen?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _repo.GetAsync(id, ct);
    }

    public async Task<(List<Screen> Items, int Total)> ListAsync(string? filter, int limit, int offset,
        CancellationToken ct = default)
    {
        var items = await _repo.ListAsync(filter, limit, offset, ct);
        // crude total
        var total = items.Count + offset; // not accurate; for demo keep it simple
        return (items, total);
    }

    public async Task UpdateLastHeartBeatAsync(Guid screenId, CancellationToken ct = default)
    {
        await _repo.UpdateLastHeartBeatAsync(screenId, ct);
    }

    public async Task<IQueryable<Screen>> GetListByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        return await _repo.GetListByUserIdAsync(userId, ct);
    }

    public async Task<Screen> UpdateAsync(Screen screen, CancellationToken ct = default)
    {
        screen.UpdatedAt = DateTime.UtcNow;
        await _repo.UpdateAsync(screen, ct);
        return screen;
    }

    public async Task DeleteAsync(Guid screenId, CancellationToken ct = default)
    {
        await _repo.DeleteAsync(screenId, ct);
    }
}