using AdControl.Domain.Models;

namespace AdControl.Application.Repository.Abstractions;

public interface IConfigRepository
{
    Task<Config?> GetAsync(Guid id, CancellationToken ct = default);
    Task<Config?> GetConfigForScreenAsync(Guid screenId, CancellationToken ct = default);
    Task<Config> CreateAsync(Config cfg, CancellationToken ct = default);
    Task AssignToScreenAsync(Guid screenId, Guid configId, bool isActive, CancellationToken ct = default);
    Task<Config?> AddItems(Guid configId, List<ConfigItem> items, CancellationToken ct = default);
}