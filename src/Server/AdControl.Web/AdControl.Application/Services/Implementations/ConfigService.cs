using AdControl.Application.Repository.Abstractions;
using AdControl.Application.Services.Abstractions;
using AdControl.Domain.Models;

namespace AdControl.Application.Services.Implementations;

public class ConfigService : IConfigService
{
    private readonly IConfigRepository _repo;

    public ConfigService(IConfigRepository repo)
    {
        _repo = repo;
    }

    public async Task<Config> CreateAsync(string name, Guid? userId, IEnumerable<ConfigItem> items, int screensCount, CancellationToken ct = default)
    {
        var cfg = new Config { Name = name, Id = Guid.NewGuid(), UserId = userId, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow, ScreensCount = screensCount};
        cfg.Items = items.Select(i =>
        {
            i.Id = i.Id == Guid.Empty ? Guid.NewGuid() : i.Id;
            i.ConfigId = cfg.Id;
            return i;
        }).ToList();

        return await _repo.CreateAsync(cfg, ct);
    }

    public async Task<Config?> GetAsync(Guid id, CancellationToken ct = default)
    {
        return await _repo.GetAsync(id, ct);
    }

    public async Task AssignToScreenAsync(Guid screenId, Guid configId, bool isActive, CancellationToken ct = default)
    {
        await _repo.AssignToScreenAsync(screenId, configId, isActive, ct);
    }

    public async Task<Config?> GetConfigForScreenAsync(Guid screenId, CancellationToken ct = default)
    {
        var cfg = await _repo.GetConfigForScreenAsync(screenId, ct);

        return cfg;
    }

    public async Task<Config?> AddItems(Guid configId, List<ConfigItem> items, CancellationToken ct = default)
    {
        var cfg = await GetAsync(configId, ct);

        if (cfg is null)
        {
            throw new NullReferenceException("Config not found");
        }

        cfg.UpdatedAt = DateTime.UtcNow;

        await UpdateAsync(cfg);

        return await _repo.AddItems(configId, items, ct);
    }

    public async Task<IEnumerable<Config?>> GetUserConfigs(Guid userId, CancellationToken ct = default)
    {
        return await _repo.GetUserConfigs(userId, ct);
    }

    public async Task<Config> UpdateAsync(Config config, CancellationToken ct = default)
    {
        config.UpdatedAt = DateTime.UtcNow;
        return await _repo.UpdateAsync(config, ct);
    }

    public async Task<bool> DeleteConfigItemAsync(Guid configId, Guid itemId, CancellationToken ct = default)
    {
        return await _repo.DeleteConfigItem(configId, itemId, ct);
    }
}