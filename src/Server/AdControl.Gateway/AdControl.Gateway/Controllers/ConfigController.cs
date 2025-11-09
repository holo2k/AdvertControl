using AdControl.Gateway.Application.Dtos;
using AdControl.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using static AdControl.Protos.AuthService;

namespace AdControl.Gateway.Controllers;

[ApiController]
[Route("api/config")]
public class ConfigController : ControllerBase
{
    private readonly AuthServiceClient _authServiceClient;
    private readonly ScreenService.ScreenServiceClient _screenClient;

    public ConfigController(ScreenService.ScreenServiceClient screenClient, AuthServiceClient authServiceClient)
    {
        _screenClient = screenClient;
        _authServiceClient = authServiceClient;
    }

    /// <summary>
    ///     Создаёт конфигурацию (Config) с элементами.
    /// </summary>
    /// <response code="201">Конфигурация создана</response>
    /// <response code="500">Ошибка при создании</response>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateConfigDto dto)
    {
        var req = new CreateConfigRequest();

        if (dto.Items != null)
            foreach (var it in dto.Items)
            {
                var type = ItemType.Image;
                if (!string.IsNullOrEmpty(it.Type) && Enum.TryParse<ItemType>(it.Type, true, out var parsed))
                    type = parsed;

                var ci = new ConfigItem
                {
                    Id = it.Id ?? Guid.CreateVersion7().ToString(),
                    ConfigId = "",
                    Type = type,
                    Url = it.Url ?? "",
                    InlineData = it.InlineData ?? "",
                    Checksum = it.Checksum ?? "",
                    Size = it.Size,
                    DurationSeconds = 5,
                    Order = it.Order
                };
                req.Items.Add(ci);
            }

        var resp = await _screenClient.CreateConfigAsync(req, BuildAuthMetadata(HttpContext)).ResponseAsync;
        if (!string.IsNullOrEmpty(resp.Error)) return StatusCode(500, new { error = resp.Error });
        return CreatedAtAction(nameof(GetById), new { id = resp.Id }, new { id = resp.Id });
    }

    /// <summary>
    ///     Получает конфигурацию по идентификатору.
    /// </summary>
    /// <response code="200">Конфигурация найдена</response>
    /// <response code="404">Конфигурация не найдена</response>
    [HttpGet("{id}")]
    [Authorize]
    [ProducesResponseType(typeof(Config), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(string id)
    {
        var resp = await _screenClient.GetConfigAsync(new GetConfigRequest { Id = id }, BuildAuthMetadata(HttpContext))
            .ResponseAsync;
        if (resp.Config == null || string.IsNullOrEmpty(resp.Config.Id)) return NotFound();
        return Ok(resp.Config);
    }

    /// <summary>
    ///     Назначает конфигурацию экрану.
    /// </summary>
    /// <response code="200">Конфигурация успешно назначена</response>
    /// <response code="500">Ошибка при назначении</response>
    [HttpPost("{id}/assign")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Assign(string id, [FromBody] AssignDto dto)
    {
        var req = new AssignConfigRequest { ScreenId = dto.ScreenId, ConfigId = id, IsActive = dto.IsActive };
        var resp = await _screenClient.AssignConfigToScreenAsync(req, BuildAuthMetadata(HttpContext)).ResponseAsync;
        if (!string.IsNullOrEmpty(resp.Error)) return StatusCode(500, new { error = resp.Error });
        return Ok(new { id = resp.Id, status = resp.Status });
    }

    [HttpPost("{id}/add-items")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddItems(string id, [FromBody] CreateConfigDto dto)
    {
        var items = dto.Items.Select(it => new ConfigItem
        {
            Id = it.Id,
            ConfigId = id, 
            Type = Enum.TryParse<ItemType>(it.Type, true, out var t)
                ? t
                : ItemType.Image,
            Url = it.Url,
            InlineData = it.InlineData,
            Checksum = it.Checksum ?? "",
            Size = it.Size,
            DurationSeconds = it.DurationSeconds,
            Order = it.Order
        }).ToList();
        var request = new AddItemsRequest
        {
            Id = id,
            Items = { items }
        };
        var response = await _screenClient.AddConfigItemsAsync(request);
        return Ok(response);
    }

    private Metadata BuildAuthMetadata(HttpContext http)
    {
        var metadata = new Metadata();
        if (http.Request.Headers.TryGetValue("Authorization", out var auth))
            metadata.Add("Authorization", auth.ToString());
        return metadata;
    }
}