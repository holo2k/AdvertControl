using AdControl.Gateway.Application.Dtos;
using AdControl.Protos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AdControl.Gateway.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService.AuthServiceClient _authServiceClient;

    public AuthController(AuthService.AuthServiceClient authServiceClient)
    {
        _authServiceClient = authServiceClient;
    }

    /// <summary>
    ///     Регистрирует нового пользователя.
    /// </summary>
    /// <param name="dto">Данные для регистрации пользователя.</param>
    /// <returns>Информация о зарегистрированном пользователе.</returns>
    [Authorize]
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Register(RegisterDto dto)
    {
        var request = new RegisterRequest
        {
            Email = dto.Username,
            Password = dto.Password,
            RepeatPassword = dto.RepeatPassword,
            Roles = { dto.Roles }
        };

        var resp = await _authServiceClient.RegisterAsync(request);
        return Ok(resp);
    }

    /// <summary>
    ///     Выполняет вход пользователя в систему.
    /// </summary>
    /// <param name="dto">Учётные данные пользователя.</param>
    /// <returns>JWT-токен для доступа к системе.</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var request = new LoginRequest
        {
            Email = dto.Username,
            Password = dto.Password
        };
        var resp = await _authServiceClient.LoginAsync(request);
        return Ok(resp);
    }

    /// <summary>
    ///     Выполняет выход пользователя из системы.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var request = new LogoutRequest
        {
            Token = token
        };
        var resp = await _authServiceClient.LogoutAsync(request);
        return Ok(resp);
    }

    /// <summary>
    ///     Получает идентификатор текущего пользователя по JWT-токену.
    /// </summary>
    [HttpPost("get-current-user-id")]
    [Authorize]
    [ProducesResponseType(typeof(UserIdResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUserId()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var request = new UserIdRequest
        {
            Token = token
        };

        var resp = await _authServiceClient.GetCurrentUserIdAsync(request);
        return Ok(resp);
    }

    [HttpPost("get-current-user-info")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCurrentUserInfo()
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var currentUserRequest = new UserIdRequest
        {
            Token = token
        };

        var currentUserIdResponse = await _authServiceClient.GetCurrentUserIdAsync(currentUserRequest);
        
        var id = currentUserIdResponse.Id;
        
        if (id is null)
            return Unauthorized();

        var userInfoRequest = new UserInfoRequest { Id = id };
        var userInfoResponse = await _authServiceClient.GetUserInfoAsync(userInfoRequest);

        return Ok(userInfoResponse);
    }

    /// <summary>
    ///     Возвращает информацию о пользователе по его идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор пользователя.</param>
    [HttpPost("get-user-info-by/{id}")]
    [Authorize]
    [ProducesResponseType(typeof(UserInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserInfo(string id)
    {
        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var requestUserId = new UserIdRequest { Token = token };
        var currentUserId = await _authServiceClient.GetCurrentUserIdAsync(requestUserId);

        if (id != currentUserId.Id)
            return Unauthorized();

        var request = new UserInfoRequest { Id = id };
        var resp = await _authServiceClient.GetUserInfoAsync(request);

        return Ok(resp);
    }
}