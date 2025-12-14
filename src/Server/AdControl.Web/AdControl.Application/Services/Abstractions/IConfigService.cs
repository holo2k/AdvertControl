using AdControl.Domain.Models;

namespace AdControl.Application.Services.Abstractions;

public interface IConfigService
{
    Task<Config> CreateAsync(string name, Guid? userId, IEnumerable<ConfigItem> items, int screensCount, CancellationToken ct = default);
    Task<Config?> GetAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Config?>> GetUserConfigs(Guid userId, CancellationToken ct = default);
    Task AssignToScreenAsync(Guid screenId, Guid configId, bool isActive, CancellationToken ct = default);
    Task<Config?> GetConfigForScreenAsync(Guid screenId, CancellationToken ct = default);
    Task<Config?> AddItems(Guid configId, List<ConfigItem> items, CancellationToken ct = default);
    Task<Config> UpdateAsync(Config config, CancellationToken ct = default);
    Task<bool> DeleteConfigItemAsync(Guid configId, Guid itemId, CancellationToken ct = default);
}