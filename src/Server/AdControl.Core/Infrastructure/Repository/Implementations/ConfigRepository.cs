using AdControl.Application.Repository.Abstractions;
using AdControl.Core.Persistence;
using AdControl.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace AdControl.Core.Infrastructure.Repository.Implementations;

public class ConfigRepository : IConfigRepository
{
    private readonly AppDbContext _db;
    private readonly IScreenRepository _screenRepository;

    public ConfigRepository(AppDbContext db, IScreenRepository screenRepository)
    {
        _db = db;
        _screenRepository = screenRepository;
    }

    public async Task<Config?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.Configs.Include(c => c.Items).FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    /// <summary>
    ///     Возвращает активную конфигурацию для указанного экрана.
    ///     Метод вызывается ТОЛЬКО ЭКРАНОМ при обращении к серверу,
    ///     т.к. в процессе обновляется время последнего Heartbeat (`LastHeartBeat`).
    /// </summary>
    /// <param name="screenId">Идентификатор экрана.</param>
    /// <param name="ct">Токен отмены.</param>
    /// <returns>Активная конфигурация экрана или null, если не найдена.</returns>
    public async Task<Config?> GetConfigForScreenAsync(Guid screenId, CancellationToken ct = default)
    {
        var screen = await _db.Screens
            .Include(s => s.ScreenConfigs)
            .ThenInclude(sc => sc.Config)
            .ThenInclude(c => c.Items)
            .Where(s => s.ScreenConfigs.Any(sc => sc.IsActive && sc.ScreenId == screenId))
            .FirstOrDefaultAsync(ct);

        if (screen is not null)
            await _screenRepository.UpdateLastHeartBeatAsync(screenId, ct);

        return screen?.ScreenConfigs
            .Where(sc => sc.IsActive && sc.ScreenId == screenId)
            .Select(sc => sc.Config)
            .FirstOrDefault();
    }

    public async Task<IEnumerable<Config?>> GetUserConfigs(Guid userId, CancellationToken ct = default)
    {
        var configs = _db.Configs.Where(config => config.UserId == userId);

        return configs;
    }

    public async Task<Config> CreateAsync(Config cfg, CancellationToken ct = default)
    {
        _db.Configs.Add(cfg);
        await _db.SaveChangesAsync(ct);
        return cfg;
    }

    public async Task AssignToScreenAsync(Guid screenId, Guid configId, bool isActive, CancellationToken ct = default)
    {
        var sc = new ScreenConfig
        {
            Id = Guid.NewGuid(),
            ScreenId = screenId,
            ConfigId = configId,
            IsActive = isActive,
            AssignedAt = DateTime.UtcNow
        };
        _db.ScreenConfigs.Add(sc);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<Config?> AddItems(Guid configId, List<ConfigItem> items, CancellationToken ct = default)
    {
        var config = await _db.Configs.Include(c => c.Items).FirstOrDefaultAsync(x => x.Id == configId, ct);

        if (config is not null)
        {
            config.UpdatedAt = DateTime.UtcNow;
        }

        _db.ConfigItems.AddRange(items);
        await _db.SaveChangesAsync(ct);
        return await GetAsync(configId, ct);
    }
    
    public async Task<Config> UpdateAsync(Config config, CancellationToken ct = default)
    {
        config.UpdatedAt = DateTime.UtcNow;
        _db.Configs.Update(config);
        await _db.SaveChangesAsync(ct);
        return config;
    }

    public async Task<bool> DeleteConfigItem(Guid configId, Guid itemId, CancellationToken ct = default)
    {
        var configItem = _db.ConfigItems.Include(ci=>ci.Config).FirstOrDefault(x=>x.ConfigId == configId && x.Id == itemId);
        if (configItem is null)
        {
            return false;
        }

        if(configItem.Config is not null)
        {
            configItem.Config.UpdatedAt = DateTime.UtcNow;
        }

        _db.ConfigItems.Remove(configItem);
        await _db.SaveChangesAsync(ct);
        return true;
    }
}