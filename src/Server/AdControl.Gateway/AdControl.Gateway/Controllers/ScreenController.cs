using AdControl.Gateway.Application.Dtos;
using AdControl.Gateway.Mapper;
using AdControl.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace AdControl.Gateway.Controllers;

[ApiController]
[Route("api/screen")]
public class ScreenController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ScreenService.ScreenServiceClient _screenClient;
    private readonly AuthService.AuthServiceClient _authServiceClient;


    public ScreenController(ScreenService.ScreenServiceClient screenClient, IConnectionMultiplexer redis, AuthService.AuthServiceClient authServiceClient)
    {
        _screenClient = screenClient;
        _redis = redis;
        _authServiceClient = authServiceClient;
    }

    /// <summary>
    ///     Создаёт экран.
    /// </summary>
    /// <response code="201">Экран успешно создан</response>
    /// <response code="500">Ошибка при создании экрана</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateScreenDto dto)
    {
        var req = new CreateScreenRequest
        {
            Name = dto.Name ?? "",
            Resolution = dto.Resolution ?? "",
            Location = dto.Location ?? ""
        };

        var metadata = BuildAuthMetadata(HttpContext);
        var call = _screenClient.CreateScreenAsync(req, metadata);
        var resp = await call.ResponseAsync;

        if (!string.IsNullOrEmpty(resp.Error))
            return StatusCode(500, new { error = resp.Error });

        return CreatedAtAction(nameof(GetById), new { id = resp.Id }, new { id = resp.Id, status = resp.Status });
    }

    /// <summary>
    ///     Получает экран по идентификатору.
    /// </summary>
    /// <response code="200">Экран найден</response>
    /// <response code="404">Экран не найден</response>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(Screen), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var req = new GetScreenRequest { Id = id };
        var metadata = BuildAuthMetadata(HttpContext);
        var resp = await _screenClient.GetScreenAsync(req, metadata).ResponseAsync;
        if (resp.Screen == null || string.IsNullOrEmpty(resp.Screen.Id))
            return NotFound();

        var cfg = resp.Config.MapToConfigDto();
        var screen = resp.Screen.MapToScreenDto();

        return Ok(new { screen, cfg } );
    }

    /// <summary>
    ///     Возвращает список экранов с фильтрацией.
    /// </summary>
    /// <response code="200">Список успешно получен</response>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] string? filterName, [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        var req = new ListScreensRequest { FilterName = filterName ?? "", Limit = limit, Offset = offset };
        var resp = await _screenClient.ListScreensAsync(req, BuildAuthMetadata(HttpContext)).ResponseAsync;

        return Ok(new { items = resp.Screens.Select(s => s.MapToScreenDto()), total = resp.Total });
    }

    /// <summary>
    ///     Возвращает информацию для дашборда
    /// </summary>
    /// <response code="200">Успешно получено</response>
    [HttpGet("dashboard")]
    [Authorize]
    [ProducesResponseType(typeof(GetDashboardResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> Dashboard()
    {
        var req = new GetDashboardRequest();
        var resp = await _screenClient.GetDashboardAsync(req, BuildAuthMetadata(HttpContext)).ResponseAsync;

        return Ok(resp);
    }


    /// <summary>
    ///     Удаление экрана (не реализовано).
    /// </summary>
    /// <response code="501">Метод не реализован</response>
    [HttpDelete("{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> Delete(string id)
    {
        var metadata = BuildAuthMetadata(HttpContext);
        var call = _screenClient.DeleteScreenAsync(new DeleteScreenRequest { Id = id }, metadata);

        var req = new ListScreensRequest { FilterName =  "", Limit = 50, Offset = 0 };
        var screensResponse = await _screenClient.ListScreensAsync(req, BuildAuthMetadata(HttpContext)).ResponseAsync;

        if (screensResponse.Screens.Any(s => s.Id == id))
        {
            var resp = await call.ResponseAsync;
            return Ok(resp);
        }

        return Unauthorized("Нельзя удалить чужой экран");
    }

    [HttpPost("update")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromBody] UpdateScreenFieldsRequest request)
    {
        var response = await _screenClient.UpdateScreenFieldsAsync(request, BuildAuthMetadata(HttpContext));
        return Ok(response.Screen.MapToScreenDto());
    }

    /// <summary>
    ///     Подтверждает код привязки экрана и создаёт экран в ScreenService.
    /// </summary>
    /// <response code="200">Привязка подтверждена</response>
    /// <response code="400">Некорректный код</response>
    /// <response code="404">Код не найден или истёк</response>
    /// <response code="500">Ошибка при создании экрана</response>
    [HttpPost("pair/confirm")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmPair([FromBody] ConfirmPairDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code)) return BadRequest();
        var db = _redis.GetDatabase();

        var key = $"pair:{dto.Code}";
        var tempId = await db.StringGetAsync(key);
        if (tempId.IsNullOrEmpty) return NotFound(new { error = "code not found or expired" });

        var createReq = new CreateScreenRequest
        {
            Name = dto.Name ?? "Unnamed",
            Resolution = dto.Resolution ?? "",
            Location = dto.Location ?? ""
        };

        var metadata = BuildAuthMetadata(HttpContext);
        var resp = await _screenClient.CreateScreenAsync(createReq, metadata).ResponseAsync;
        if (!string.IsNullOrEmpty(resp.Error)) return StatusCode(500, new { error = resp.Error });

        var assignedKey = $"pair:assigned:{tempId}";
        await db.StringSetAsync(assignedKey, resp.Id,
            TimeSpan.FromMinutes(dto.AssignedTtlMinutes > 0 ? dto.AssignedTtlMinutes : 60));

        await db.KeyDeleteAsync(key);
        await db.KeyDeleteAsync($"pair:meta:{tempId}");

        return Ok(new { id = resp.Id, status = resp.Status });
    }

    private Metadata BuildAuthMetadata(HttpContext http)
    {
        var metadata = new Metadata();
        if (http.Request.Headers.TryGetValue("Authorization", out var auth))
            metadata.Add("Authorization", auth.ToString());
        return metadata;
    }

    [HttpGet("get-history")]
    [Authorize]
    public async Task<IActionResult> GetHistory()
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var currentUserRequest = new UserIdRequest
        {
            Token = token
        };
        
        var currentUser = _authServiceClient.GetCurrentUserId(currentUserRequest);
        var historyRequest = new ListUserScreensRequest
        {
            UserId = currentUser.Id,
        };
        var screenListByUserId = await _screenClient.GetListUserScreensAsync(historyRequest);
        return Ok(screenListByUserId);
    } 
}