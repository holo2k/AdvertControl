using System.Data;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AdControl.Auth.Models;
using AdControl.Core.Constants;
using AdControl.Protos;
using Microsoft.Extensions.Options;

namespace AdControl.Auth;

public class KeycloakSetupService : IKeycloakSetupService
{
    private readonly string _defaultAdminPassword;
    private readonly string _defaultAdminUsername;
    private readonly string _defaultClientId;
    private readonly string _defaultRealm;
    private readonly HttpClient _httpClient;
    private readonly string _keycloakBaseUrl;

    public KeycloakSetupService(HttpClient httpClient, IOptions<KeycloakOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        var o = options.Value;

        _defaultAdminUsername = o.AdminUser ?? "admin";
        _defaultAdminPassword = o.AdminPassword ?? "qwerty";
        _defaultRealm = Environment.GetEnvironmentVariable("KEYCLOAK_REALM") ?? "myrealm";
        _defaultClientId = "app-client"; // клиент для всех пользователей
        _keycloakBaseUrl =
            o.BaseUrl ?? Environment.GetEnvironmentVariable("KEYCLOAK_BASEURL") ?? "http://keycloak:8080";

        Trace.WriteLine($"BaseUrl : {_keycloakBaseUrl}\nDefaultRealm : {_defaultRealm}\n");
    }

    public async Task<string?> GetCurrentUserIdAsync(string token)
    {
        //var masterToken = await AcquireMasterTokenAsync();
        var url = $"{_keycloakBaseUrl}/realms/{_defaultRealm}/protocol/openid-connect/userinfo";
        using var req = new HttpRequestMessage(HttpMethod.Get, url)
        {
            Headers =
            {
                Authorization = new AuthenticationHeaderValue("Bearer", token)
            }
        };

        var resp = await _httpClient.SendAsync(req);
        var result = resp.Content.ReadAsStringAsync();
        if (!resp.IsSuccessStatusCode) return null;

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        var userId = doc.RootElement.GetProperty("sub").GetString();

        return userId;
    }

    public async Task<JsonElement?> GetUserByIdAsync(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId)) return null;

        var masterToken = await AcquireMasterTokenAsync();
        var url = $"{_keycloakBaseUrl}/admin/realms/{_defaultRealm}/users/{userId}";

        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", masterToken);

        using var resp = await _httpClient.SendAsync(req);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.Clone(); // клон, безопасно вернуть после dispose
    }


    public async Task EnsureSetupAsync()
    {
        var masterToken = await AcquireMasterTokenAsync();

        // 1. Создать realm если не существует
        var realmUrl = $"{_keycloakBaseUrl}/admin/realms/{Uri.EscapeDataString(_defaultRealm)}";
        using (var getReq = new HttpRequestMessage(HttpMethod.Get, realmUrl))
        {
            getReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", masterToken);
            var resp = await _httpClient.SendAsync(getReq);
            if (!resp.IsSuccessStatusCode && resp.StatusCode != HttpStatusCode.NotFound)
                throw new InvalidOperationException(
                    $"Error checking realm: {resp.StatusCode} {await resp.Content.ReadAsStringAsync()}");

            if (resp.StatusCode == HttpStatusCode.NotFound)
            {
                var createRealmUrl = $"{_keycloakBaseUrl}/admin/realms";
                var realmObj = new { realm = _defaultRealm, enabled = true, registrationAllowed = true };
                using var createReq = new HttpRequestMessage(HttpMethod.Post, createRealmUrl)
                {
                    Content = new StringContent(JsonSerializer.Serialize(realmObj), Encoding.UTF8, "application/json")
                };
                createReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", masterToken);
                var createResp = await _httpClient.SendAsync(createReq);
                createResp.EnsureSuccessStatusCode();
            }
        }

        // 2. Создать client app-client если не существует
        var clientsUrl = $"{_keycloakBaseUrl}/admin/realms/{_defaultRealm}/clients";
        using (var getClientsReq = new HttpRequestMessage(HttpMethod.Get, clientsUrl))
        {
            getClientsReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", masterToken);
            var resp = await _httpClient.SendAsync(getClientsReq);
            resp.EnsureSuccessStatusCode();
            var clients = JsonSerializer.Deserialize<List<JsonElement>>(await resp.Content.ReadAsStringAsync())!;
            var client = clients.FirstOrDefault(c => c.GetProperty("clientId").GetString() == _defaultClientId);
            if (client.ValueKind == JsonValueKind.Undefined)
            {
                var clientObj = new
                {
                    clientId = _defaultClientId,
                    enabled = true,
                    publicClient = true,
                    directAccessGrantsEnabled = true,
                    standardFlowEnabled = true
                };
                using var createClientReq = new HttpRequestMessage(HttpMethod.Post, clientsUrl)
                {
                    Content = new StringContent(JsonSerializer.Serialize(clientObj), Encoding.UTF8, "application/json")
                };
                createClientReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", masterToken);
                var createResp = await _httpClient.SendAsync(createClientReq);
                createResp.EnsureSuccessStatusCode();
            }
            else
            {
                var id = client.GetProperty("id").GetString()!;
                var updateUrl = $"{clientsUrl}/{id}";
                var updateObj = new
                {
                    id,
                    clientId = _defaultClientId,
                    enabled = true,
                    publicClient = true,
                    directAccessGrantsEnabled = true,
                    standardFlowEnabled = true
                };
                using var updateReq = new HttpRequestMessage(HttpMethod.Put, updateUrl)
                {
                    Content = new StringContent(JsonSerializer.Serialize(updateObj), Encoding.UTF8, "application/json")
                };
                updateReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", masterToken);
                var updateResp = await _httpClient.SendAsync(updateReq);
                updateResp.EnsureSuccessStatusCode();
            }
        }

        // 3. Создать роль Admin если не существует
        var rolesUrl = $"{_keycloakBaseUrl}/admin/realms/{_defaultRealm}/roles";
        using (var rolesReq = new HttpRequestMessage(HttpMethod.Get, rolesUrl))
        {
            rolesReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", masterToken);
            var rolesResp = await _httpClient.SendAsync(rolesReq);
            rolesResp.EnsureSuccessStatusCode();
            var rolesBody = await rolesResp.Content.ReadAsStringAsync();
            var roles = JsonSerializer.Deserialize<List<JsonElement>>(rolesBody)!;
            if (!roles.Any(r => r.GetProperty("name").GetString() == "Admin"))
            {
                using var createRoleReq = new HttpRequestMessage(HttpMethod.Post, rolesUrl)
                {
                    Content = new StringContent(JsonSerializer.Serialize(new { name = "Admin" }), Encoding.UTF8,
                        "application/json")
                };
                createRoleReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", masterToken);
                var roleResp = await _httpClient.SendAsync(createRoleReq);
                roleResp.EnsureSuccessStatusCode();
            }
        }

        // 4. Создать дефолтного админа и назначить роль Admin
        var request = new CreateUserRequest()
        {
            Email = _defaultAdminUsername,
            Password =  _defaultAdminPassword,
            Roles = new[] { RolesConstants.Admin },
            Name = "Admin",
            SecondName = "",
            MasterToken = masterToken,
            Phone = "",
        };

        await CreateUserIfNotExistsAsync(request);
    }

    public async Task CreateUserAsync(RegisterRequest req, string? realmName = null)
    {
        await EnsureSetupAsync();

        var masterToken = await AcquireMasterTokenAsync();
        var roles = req.Roles.ToArray();

        var request = new CreateUserRequest()
        {
            Email = req.Email,
            Password = req.Password,
            Roles = roles.Length > 0 ? roles : new[] { RolesConstants.User },
            Name = req.Name,
            SecondName = req.SecondName,
            MasterToken = masterToken,
            Phone = req.Phone,
        };

        await CreateUserIfNotExistsAsync(request);
    }

    public async Task<bool> LogoutAsync(string accessToken)
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"{_keycloakBaseUrl}/realms/{_defaultRealm}/protocol/openid-connect/logout",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "token", accessToken }
                })
            );

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error during logout: {ex.Message}");
            return false;
        }
    }


    public async Task<string> GetJwtTokenAsync(string username, string password, string? realmName = null)
    {
        var realm = realmName ?? _defaultRealm;
        var url = $"{_keycloakBaseUrl}/realms/{Uri.EscapeDataString(realm)}/protocol/openid-connect/token";
        var form = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("client_id", _defaultClientId),
            new("username", username),
            new("password", password),
            new("scope", "openid profile email")
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = new FormUrlEncodedContent(form)
        };

        var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }


    private async Task CreateUserIfNotExistsAsync(CreateUserRequest request)
    {
        var usersUrl =
            $"{_keycloakBaseUrl}/admin/realms/{_defaultRealm}/users?username={Uri.EscapeDataString(request.Email)}";
        using var getUserReq = new HttpRequestMessage(HttpMethod.Get, usersUrl);
        getUserReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.MasterToken);
        var getUserResp = await _httpClient.SendAsync(getUserReq);
        getUserResp.EnsureSuccessStatusCode();
        var users = JsonSerializer.Deserialize<List<JsonElement>>(await getUserResp.Content.ReadAsStringAsync())!;

        string userId;
        if (users.Any())
        {
            userId = users[0].GetProperty("id").GetString()!;
        }
        else
        {
            var userObj = new
            {
                request.Email,
                enabled = true,
                emailVerified = true,

                firstName = request.Name,
                lastName = request.SecondName,

                credentials = new[]
                {
                    new { type = "password", value = request.Password, temporary = false }
                },

                attributes = new
                {
                    phoneNumber = new[] { request.Phone }
                }
            };

            using var createUserReq =
                new HttpRequestMessage(HttpMethod.Post, $"{_keycloakBaseUrl}/admin/realms/{_defaultRealm}/users")
                {
                    Content = new StringContent(JsonSerializer.Serialize(userObj), Encoding.UTF8, "application/json")
                };
            createUserReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.MasterToken);
            var createUserResp = await _httpClient.SendAsync(createUserReq);
            createUserResp.EnsureSuccessStatusCode();

            // получить id созданного пользователя
            using var getNewUserReq = new HttpRequestMessage(HttpMethod.Get, usersUrl);
            getNewUserReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.MasterToken);
            var newUserResp = await _httpClient.SendAsync(getNewUserReq);
            newUserResp.EnsureSuccessStatusCode();
            var newUsers =
                JsonSerializer.Deserialize<List<JsonElement>>(await newUserResp.Content.ReadAsStringAsync())!;
            userId = newUsers[0].GetProperty("id").GetString()!;
        }

        await AssignRolesAsync(userId, request.Roles, request.MasterToken);
    }

    public async Task UpdateUserAsync(string userId, string? email = null, string? firstName = null,
        string? lastName = null, string? phoneNumber = null)
    {
        var masterToken = await AcquireMasterTokenAsync();

        if (string.IsNullOrWhiteSpace(userId))
            throw new ArgumentException("UserId is required", nameof(userId));

        var updateObj = new
        {
            email,
            firstName,
            lastName,
            enabled = true,
            emailVerified = true,
            attributes = new
            {
                phoneNumber = phoneNumber is not null ? new[] { phoneNumber } : Array.Empty<string>()
            }
        };

        var url = $"{_keycloakBaseUrl}/admin/realms/{_defaultRealm}/users/{userId}";
        using var req = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = new StringContent(JsonSerializer.Serialize(updateObj), Encoding.UTF8, "application/json")
        };
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", masterToken);

        var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();
    }


    private async Task AssignRolesAsync(string userId, string[] roles, string masterToken)
    {
        var rolesUrl = $"{_keycloakBaseUrl}/admin/realms/{_defaultRealm}/roles";
        using var req = new HttpRequestMessage(HttpMethod.Get, rolesUrl);
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", masterToken);
        var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        var allRoles = JsonSerializer.Deserialize<List<JsonElement>>(await resp.Content.ReadAsStringAsync())!;

        var rolesToAssign = allRoles
            .Where(r => roles.Contains(r.GetProperty("name").GetString()))
            .Select(r => new { id = r.GetProperty("id").GetString(), name = r.GetProperty("name").GetString() })
            .ToList();

        if (!rolesToAssign.Any()) return;

        var assignUrl = $"{_keycloakBaseUrl}/admin/realms/{_defaultRealm}/users/{userId}/role-mappings/realm";
        using var assignReq = new HttpRequestMessage(HttpMethod.Post, assignUrl)
        {
            Content = new StringContent(JsonSerializer.Serialize(rolesToAssign), Encoding.UTF8, "application/json")
        };
        assignReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", masterToken);
        var assignResp = await _httpClient.SendAsync(assignReq);
        assignResp.EnsureSuccessStatusCode();
    }

    private async Task<string> AcquireMasterTokenAsync()
    {
        var tokenUrl = $"{_keycloakBaseUrl}/realms/master/protocol/openid-connect/token";
        var form = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "password"),
            new("client_id", "admin-cli"),
            new("username", Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN") ?? "admin"),
            new("password", Environment.GetEnvironmentVariable("KEYCLOAK_ADMIN_PASSWORD") ?? "admin")
        };

        using var req = new HttpRequestMessage(HttpMethod.Post, tokenUrl)
        {
            Content = new FormUrlEncodedContent(form)
        };
        var resp = await _httpClient.SendAsync(req);
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("access_token").GetString()!;
    }
}