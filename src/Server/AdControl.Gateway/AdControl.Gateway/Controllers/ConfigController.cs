using AdControl.Gateway.Application.Dtos;
using AdControl.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreateConfigDto dto)
    {
        var req = new CreateConfigRequest { UserId = dto.UserId ?? "" };

        if (dto.Items != null)
            foreach (var it in dto.Items)
            {
                var type = ItemType.Image;
                if (!string.IsNullOrEmpty(it.Type) && Enum.TryParse<ItemType>(it.Type, true, out var parsed))
                    type = parsed;

                var ci = new ConfigItem
                {
                    Id = it.Id ?? Guid.NewGuid().ToString(),
                    ConfigId = "", // сервис создаст
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

        var resp = await _screenClient.CreateConfigAsync(req, BuildAuthMetadata(HttpContext)).ResponseAsync;
        if (!string.IsNullOrEmpty(resp.Error)) return StatusCode(500, new { error = resp.Error });
        return CreatedAtAction(nameof(GetById), new { id = resp.Id }, new { id = resp.Id });
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(string id)
    {
        var resp = await _screenClient.GetConfigAsync(new GetConfigRequest { Id = id }, BuildAuthMetadata(HttpContext))
            .ResponseAsync;
        if (resp.Config == null || string.IsNullOrEmpty(resp.Config.Id)) return NotFound();
        return Ok(resp.Config);
    }

    [HttpPost("{id}/assign")]
    [Authorize]
    public async Task<IActionResult> Assign(string id, [FromBody] AssignDto dto)
    {
        var req = new AssignConfigRequest { ScreenId = dto.ScreenId, ConfigId = id, IsActive = dto.IsActive };
        var resp = await _screenClient.AssignConfigToScreenAsync(req, BuildAuthMetadata(HttpContext)).ResponseAsync;
        if (!string.IsNullOrEmpty(resp.Error)) return StatusCode(500, new { error = resp.Error });
        return Ok(new { id = resp.Id, status = resp.Status });
    }

    private Metadata BuildAuthMetadata(HttpContext http)
    {
        var metadata = new Metadata();
        if (http.Request.Headers.TryGetValue("Authorization", out var auth))
            metadata.Add("Authorization", auth.ToString());
        return metadata;
    }
}