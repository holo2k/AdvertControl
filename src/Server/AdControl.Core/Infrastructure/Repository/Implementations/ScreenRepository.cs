using AdControl.Application.Repository.Abstractions;
using AdControl.Core.Persistence;
using AdControl.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AdControl.Core.Infrastructure.Repository.Implementations;

public class ScreenRepository : IScreenRepository
{
    private readonly AppDbContext _db;

    public ScreenRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Screen?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Screens.Include(s => s.ScreenConfigs).ThenInclude(sc => sc.Config).ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<List<Screen>> ListAsync(string? filterName, int limit, int offset, CancellationToken ct = default)
    {
        var q = _db.Screens.AsQueryable();
        if (!string.IsNullOrWhiteSpace(filterName)) q = q.Where(x => x.Name.Contains(filterName));
        return await q.OrderBy(x => x.Name).Skip(offset).Take(limit).ToListAsync(ct);
    }

    public async Task<Screen> CreateAsync(Screen screen, CancellationToken ct = default)
    {
        _db.Screens.Add(screen);
        await _db.SaveChangesAsync(ct);
        return screen;
    }

    public async Task UpdateAsync(Screen screen, CancellationToken ct = default)
    {
        _db.Screens.Update(screen);
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateLastHeartBeatAsync(Guid screenId, CancellationToken ct = default)
    {
        var screen = await _db.Screens.FirstOrDefaultAsync(x => x.Id == screenId, ct);
        if (screen is not null)
            screen.LastHeartbeatAt = DateTime.UtcNow;
    }

    public async Task<IQueryable<Screen>> GetListByUserIdAsync(Guid userId, CancellationToken ct = default)
    {
        var screens = _db.Screens.Where(x=>x.UserId == userId);
        return screens;
    }

}