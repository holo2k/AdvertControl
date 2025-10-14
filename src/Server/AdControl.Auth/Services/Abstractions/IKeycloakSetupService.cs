namespace AdControl.Auth;

public interface IKeycloakSetupService
{
    Task EnsureSetupAsync();
    Task<string> GetJwtTokenAsync(string username, string password, string? realmName = null);
    Task CreateUserAsync(string username, string password, string[] roles, string realmName);
    Task<bool> LogoutAsync(string accessToken);
}