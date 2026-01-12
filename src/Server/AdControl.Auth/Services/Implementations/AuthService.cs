using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using AdControl.Protos;
using Grpc.Core;

namespace AdControl.Auth;

public class AuthService : Protos.AuthService.AuthServiceBase
{
    private readonly IKeycloakSetupService _keycloakSetupService;

    public AuthService(IKeycloakSetupService keycloakSetupService)
    {
        _keycloakSetupService = keycloakSetupService;
    }

    public override async Task<RegisterResponse> Register(RegisterRequest registerRequest, ServerCallContext context)
    {
        var realmName = "myrealm";

        await _keycloakSetupService.CreateUserAsync(registerRequest, realmName);
        var jwtToken = await _keycloakSetupService.GetJwtTokenAsync(registerRequest.Email, registerRequest.Password, realmName);
        return new RegisterResponse
        {
            Token = jwtToken
        };
    }

    public override async Task<LoginResponse> Login(LoginRequest loginRequest, ServerCallContext context)
    {
        var username = loginRequest.Email;
        var password = loginRequest.Password;
        var realmName = "myrealm";

        var jwtToken = await _keycloakSetupService.GetJwtTokenAsync(username, password, realmName);
        return new LoginResponse
        {
            Token = jwtToken
        };
    }


    public override async Task<LogoutResponse> Logout(LogoutRequest logoutRequest, ServerCallContext context)
    {
        var accessToken = logoutRequest.Token;
        var logoutResponse = await _keycloakSetupService.LogoutAsync(accessToken);

        if (logoutResponse) return new LogoutResponse { Success = true };

        throw new RpcException(new Status(StatusCode.Internal, "Logout failed"));
    }

    public override async Task<UserIdResponse> GetCurrentUserId(UserIdRequest request, ServerCallContext context)
    {
        var token = request.Token;
        var userId = await _keycloakSetupService.GetCurrentUserIdAsync(token);

        return new UserIdResponse
        {
            Id = userId ?? ""
        };
    }

    public override async Task<UpdateUserResponse> UpdateUser(UpdateUserRequest request, ServerCallContext context)
    {
        try
        {
            var userIdString = GetUserIdFromMetadata(context);

            if (userIdString != request.Id)
                throw new UnauthorizedAccessException();

            await _keycloakSetupService.UpdateUserAsync(request.Id, request.Email, request.FirstName,
                request.LastName, request.PhoneNumber);
        }
        catch (Exception ex)
        {
            return new UpdateUserResponse { Success = false, Message = ex.Message };
        }

        return new UpdateUserResponse { Success = true, Message = $"User {request.Id} updated" };
    }

    public override async Task<UserInfoResponse> GetUserInfo(UserInfoRequest request, ServerCallContext context)
    {
        var userJson = await _keycloakSetupService.GetUserByIdAsync(request.Id);
        if (userJson == null)
            return new UserInfoResponse();

        return ParseUser(userJson.Value);
    }

    public override async Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
    {
        var usersJson = await _keycloakSetupService.GetUsersAsync();
        if (usersJson == null || usersJson.Value.ValueKind != JsonValueKind.Array)
            return new GetUsersResponse();

        var response = new GetUsersResponse();

        foreach (var root in usersJson.Value.EnumerateArray())
        {
            response.Users.Add(ParseUser(root));
        }

        return response;
    }

    private static string? GetUserIdFromMetadata(ServerCallContext context)
    {
        var authEntry =
            context.RequestHeaders.FirstOrDefault(h => h.Key == "authorization" || h.Key == "authorization-bin");
        if (authEntry == null)
            return null;

        var auth = authEntry.Value;
        if (string.IsNullOrEmpty(auth))
            return null;

        // "Bearer <token>"
        var token = auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? auth.Substring(7) : auth;

        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token))
            return null;
        var jwt = handler.ReadJwtToken(token);

        var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;
        return sub;
    }

    private UserInfoResponse ParseUser(JsonElement root)
    {
        static string GetString(JsonElement el, string name)
        {
            return el.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.String
                ? prop.GetString() ?? string.Empty
                : string.Empty;
        }

        // userId
        var userId = root.TryGetProperty("id", out var idProp) && idProp.ValueKind == JsonValueKind.String
            ? idProp.GetString() ?? string.Empty
            : string.Empty;

        // phone
        var phone = string.Empty;
        if (root.TryGetProperty("attributes", out var attr) &&
            attr.ValueKind == JsonValueKind.Object &&
            attr.TryGetProperty("phoneNumber", out var phoneProp) &&
            phoneProp.ValueKind == JsonValueKind.Array &&
            phoneProp.EnumerateArray().Any())
        {
            phone = phoneProp.EnumerateArray().First().GetString() ?? string.Empty;
        }

        // role — первая непустая строка из roles[]
        var role = string.Empty;
        if (root.TryGetProperty("roles", out var rolesProp) && rolesProp.ValueKind == JsonValueKind.Array)
        {
            role = rolesProp.EnumerateArray()
                .Select(r => r.ValueKind == JsonValueKind.String ? r.GetString() : null)
                .FirstOrDefault(s => !string.IsNullOrEmpty(s)) ?? string.Empty;
        }

        return new UserInfoResponse
        {
            UserId = userId,
            Username = GetString(root, "username"),
            Role = role,
            Email = GetString(root, "email"),
            FirstName = GetString(root, "firstName"),
            LastName = GetString(root, "lastName"),
            PhoneNumber = phone,
            Enabled = root.TryGetProperty("enabled", out var enabledProp) && enabledProp.ValueKind == JsonValueKind.True
        };
    }
}