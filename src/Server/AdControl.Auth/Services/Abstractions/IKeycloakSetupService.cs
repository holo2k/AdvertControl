using System.Text.Json;

namespace AdControl.Auth;

public interface IKeycloakSetupService
{
    Task EnsureSetupAsync();
    Task<string> GetJwtTokenAsync(string username, string password, string? realmName = null);
    Task CreateUserAsync(string username, string password, string[] roles, string realmName);

    Task UpdateUserAsync(string userId, string? email = null, string? firstName = null,
        string? lastName = null, string? phoneNumber = null);

    Task<bool> LogoutAsync(string accessToken);
    public Task<string?> GetCurrentUserIdAsync(string token);
    public Task<JsonElement?> GetUserByIdAsync(string userId);
}