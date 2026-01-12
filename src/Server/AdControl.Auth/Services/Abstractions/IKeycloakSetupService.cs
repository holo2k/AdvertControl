using System.Text.Json;
using AdControl.Protos;

namespace AdControl.Auth;

public interface IKeycloakSetupService
{
    Task EnsureSetupAsync();
    Task<string> GetJwtTokenAsync(string username, string password, string? realmName = null);
    Task CreateUserAsync(RegisterRequest req, string realmName);

    Task UpdateUserAsync(string userId, string? email = null, string? firstName = null,
        string? lastName = null, string? phoneNumber = null);

    Task<bool> LogoutAsync(string accessToken);
    public Task<string?> GetCurrentUserIdAsync(string token);
    public Task<JsonElement?> GetUserByIdAsync(string userId);
    public Task<JsonElement?> GetUsersAsync();
}