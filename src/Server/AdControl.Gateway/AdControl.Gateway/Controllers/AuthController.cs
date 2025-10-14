using AdControl.Gateway.Application.Dtos;
using AdControl.Protos;
using Microsoft.AspNetCore.Mvc;


namespace AdControl.Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService.AuthServiceClient authServiceClient;

    public AuthController(AuthService.AuthServiceClient authServiceClient)
    {
        this.authServiceClient = authServiceClient;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var request = new RegisterRequest
        {
            Email = dto.Username,
            Password = dto.Password,
            RepeatPassword = dto.RepeatPassword,
        };
        
        var resp =  await authServiceClient.RegisterAsync(request);
        return Ok(resp);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var request = new LoginRequest
        {
            Email = dto.Username,
            Password = dto.Password,
        };
        var resp = await authServiceClient.LoginAsync(request);
        return Ok(resp);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutDto dto)
    {
        var request = new LogoutRequest
        {
            Token = dto.Token
        };
        var resp = await authServiceClient.LogoutAsync(request);
        return Ok(resp);
    }
}