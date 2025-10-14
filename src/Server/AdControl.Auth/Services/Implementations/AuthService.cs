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
        var username = registerRequest.Email;
        var password = registerRequest.Password;
        var realmName = "myrealm";

        await _keycloakSetupService.CreateUserAsync(username, password, registerRequest.Roles.ToArray(), realmName);
        var jwtToken = await _keycloakSetupService.GetJwtTokenAsync(username, password, realmName);
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
}