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
        return await _db.Screens.Include(s => s.ScreenConfigs).ThenInclude(sc => sc.Config)
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
}