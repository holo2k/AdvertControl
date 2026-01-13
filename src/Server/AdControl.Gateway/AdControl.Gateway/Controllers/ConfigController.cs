using AdControl.Gateway.Application.Dtos;
using AdControl.Gateway.Mapper;
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
    private readonly ScreenService.ScreenServiceClient _screenClient;

    public ConfigController(ScreenService.ScreenServiceClient screenClient)
    {
        _screenClient = screenClient;
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
                else
                    return BadRequest($"Указан неправильный тип при создании элемента конфига {it.Url}");

                var ci = new ConfigItem
                {
                    Id = it.Id ?? Guid.NewGuid().ToString(),
                    ConfigId = "",
                    Type = type,
                    Url = it.Url ?? "",
                    InlineData = it.InlineData ?? "",
                    Checksum = it.Checksum ?? "",
                    Size = it.Size,
                    DurationSeconds = it.DurationSeconds,
                    Order = it.Order
                };

                req.Items.Add(ci);
            }

        req.Name = dto.Name;
        req.ScreensCount = dto.ScreensCount;
        req.IsStatic = dto.IsStatic;

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
        return Ok(resp.Config.MapToConfigDto());
    }

    /// <summary>
    ///     Получает конфигураци текущего пользователя.
    /// </summary>
    /// <response code="200">Конфигурация найдена</response>
    /// <response code="404">Конфигурация не найдена</response>
    [HttpGet("current")]
    [Authorize]
    [ProducesResponseType(typeof(List<Config>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrentUserConfigs()
    {
        var resp = await _screenClient.GetConfigsAsync(new GetConfigsRequest(), BuildAuthMetadata(HttpContext));
        if (resp.Configs == null || resp.Configs.Count == 0)
            return NotFound();
        return Ok(resp.Configs.Select(c => c.MapToConfigDto()));
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
        var response = await _screenClient.AddConfigItemsAsync(request, BuildAuthMetadata(HttpContext));
        return Ok(response);
    }

    [Obsolete("Метод устарел. Используйте /update-config")]
    [HttpPatch("{id}/update")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateConfigDto dto)
    {
        var request = new UpdateConfigRequest
        {
            Id = id,
            Name = dto.Name,
            ScreensCount = dto.ScreensCount,
            IsStatic = dto.IsStatic,
        };

        var response = await _screenClient.UpdateConfigFieldsAsync(request, BuildAuthMetadata(HttpContext));
        return Ok(response);
    }

    [HttpPost("{id}/update-config")]
    [Authorize]
    public async Task<IActionResult> UpdateConfig(string id, [FromBody] ConfigDto dto)
    {
        var protoItems = dto.Items.Select(i => new ConfigItem
        {
            Id = i.Id.ToString(),
            ConfigId = i.Id.ToString(), 
            Type = Enum.TryParse<ItemType>(i.Type, true, out var t)
                ? t
                : ItemType.Image,
            Url = i.Url,
            InlineData = i.InlineData,
            Checksum = i.Checksum,
            Size = i.Size,
            DurationSeconds = i.DurationSeconds,
            Order = i.Order
        });
        
        var request = new Config
        {
            CreatedAt = DateTimeToUnixMs(dto.CreatedAt),
            Name = dto.Name,
            ScreensCount = dto.ScreensCount,
            IsStatic = dto.IsStatic,
            UpdatedAt = DateTimeToUnixMs(dto.UpdatedAt),
            UserId = dto.UserId?.ToString() ?? "",
            Version = dto.Version,
            Items = { protoItems }
        };

        var response = await _screenClient.UpdateConfigAsync(request);
        return Ok(response);
    }
    
    [HttpDelete("{id}/remove-items")]
    [Authorize]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveItem(string id, [FromBody] string itemId)
    {
        var configRequest = new GetConfigRequest { Id = id };
        var configResponse = await _screenClient.GetConfigAsync(configRequest, BuildAuthMetadata(HttpContext)).ResponseAsync;
        var removeItemRequest = new RemoveItemRequest
        {
            Id = id,
            ItemId = itemId
        };
        var removeItemResponse = await _screenClient.RemoveConfigItemAsync(removeItemRequest, BuildAuthMetadata(HttpContext));
        return Ok(removeItemResponse);
    }

    private Metadata BuildAuthMetadata(HttpContext http)
    {
        var metadata = new Metadata();
        if (http.Request.Headers.TryGetValue("Authorization", out var auth))
            metadata.Add("Authorization", auth.ToString());
        return metadata;
    }
    
    private static long DateTimeToUnixMs(DateTime dt)
    {
        return new DateTimeOffset(dt.ToUniversalTime()).ToUnixTimeMilliseconds();
    }
}