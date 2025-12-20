using System.Text.Json;
using AdControl.Gateway.Application.Dtos;
using AdControl.Gateway.Application.Minio;
using AdControl.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Minio;
using StackExchange.Redis;

namespace AdControl.Gateway.Controllers;

/// <summary>
///     Контроллер для взаимодействия экранов с сервером. Используется самим экраном и для тестирования.
/// </summary>
[ApiController]
[Route("api/polling")]
[Produces("application/json")]
public class PollingController : ControllerBase
{
    private readonly AvaloniaLogicService.AvaloniaLogicServiceClient _avaloniaClient;
    private readonly IMinioClient _minio;
    private readonly MinioSettings _minioSettings;
    private readonly IConnectionMultiplexer _redis;

    public PollingController(AvaloniaLogicService.AvaloniaLogicServiceClient avaloniaClient,
        IConnectionMultiplexer redis, IMinioClient minio, IOptions<MinioSettings> options)
    {
        _avaloniaClient = avaloniaClient;
        _redis = redis;
        _minio = minio;
        _minioSettings = options.Value;
    }

    /// <summary>
    ///     Проверяет, привязан ли экран по идентификатору.
    /// </summary>
    /// <param name="id">Идентификатор экрана.</param>
    /// <returns>True, если экран привязан.</returns>
    [HttpGet("is-assigned/{id}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> IsAssigned(string id)
    {
        var req = new IsScreenExistRequest { ScreenId = id };
        var resp = await _avaloniaClient.IsScreenExistAsync(req).ResponseAsync;
        if (resp is not { IsExist: true })
            return NotFound(false);
        return Ok(true);
    }

    /// <summary>
    ///     Начинает процесс привязки экрана (pairing).
    /// </summary>
    /// <param name="dto">Данные для начала привязки.</param>
    /// <returns>Информация о созданной привязке.</returns>
    [HttpPost("pair/start")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> StartPair([FromBody] StartPairDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TempDisplayId) || string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { error = "TempDisplayId and Code required" });

        var db = _redis.GetDatabase();
        var key = $"pair:{dto.Code}";
        var set = await db.StringSetAsync(key, dto.TempDisplayId,
            TimeSpan.FromMinutes(dto.TtlMinutes > 0 ? dto.TtlMinutes : 10), When.NotExists);
        if (!set)
            return Conflict(new { error = "Code already in use" });

        if (!string.IsNullOrEmpty(dto.Info))
            await db.StringSetAsync($"pair:meta:{dto.TempDisplayId}", dto.Info,
                TimeSpan.FromMinutes(dto.TtlMinutes > 0 ? dto.TtlMinutes : 10));

        return Ok(new { ok = true, code = dto.Code, tempDisplayId = dto.TempDisplayId });
    }

    /// <summary>
    ///     Проверяет статус привязки по временному идентификатору экрана.
    /// </summary>
    /// <param name="tempDisplayId">Временный идентификатор экрана.</param>
    /// <returns>True, если экран привязан.</returns>
    [HttpGet("pair/status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PairStatus([FromQuery] string tempDisplayId)
    {
        if (string.IsNullOrWhiteSpace(tempDisplayId)) return BadRequest();

        var db = _redis.GetDatabase();
        var assignedKey = $"pair:assigned:{tempDisplayId}";
        var screenId = await db.StringGetAsync(assignedKey);
        if (screenId.IsNullOrEmpty) return Ok(new { assigned = false });

        await db.KeyDeleteAsync(assignedKey);

        return Ok(new { assigned = true, screenId = screenId.ToString() });
    }

    /// <summary>
    ///     Возвращает конфигурацию для указанного экрана.
    /// </summary>
    /// <param name="screenId">Идентификатор экрана.</param>
    /// <param name="knownVersion">Известная версия конфигурации (для проверки изменений).</param>
    /// <returns>Конфигурация экрана или 304, если изменений нет.</returns>
    [HttpGet("config")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetConfig([FromQuery] string screenId, [FromQuery] long knownVersion = 0)
    {
        if (string.IsNullOrWhiteSpace(screenId)) return BadRequest();

        var db = _redis.GetDatabase();

        try
        {
            var req = new GetConfigForScreenRequest { ScreenId = screenId, KnownVersion = knownVersion };
            var grpcResp = await _avaloniaClient.GetConfigForScreenAsync(req).ResponseAsync;

            if (!string.IsNullOrEmpty(grpcResp.Error))
                return StatusCode(StatusCodes.Status404NotFound, new { error = grpcResp.Error });

            if (grpcResp.NotModified)
                return StatusCode(StatusCodes.Status304NotModified);

            var cfg = grpcResp.Config;

            var items = cfg.Items.Select(it => new
            {
                id = it.Id,
                type = it.Type.ToString(),
                url = it.Url,
                inlineData = it.InlineData,
                checksum = it.Checksum,
                size = it.Size,
                durationSeconds = it.DurationSeconds,
                order = it.Order
            }).ToList();

            var cacheObj = new { version = cfg.Version, items, updatedAt = cfg.UpdatedAt };
            var cacheJson = JsonSerializer.Serialize(cacheObj);
            await db.StringSetAsync($"config:screen:{screenId}", cacheJson);

            return Ok(new { version = cfg.Version, items, cfg.ScreensCount, cfg.UpdatedAt, cfg.IsStatic });
        }
        catch (RpcException)
        {
            // fallback
        }
        catch (Exception)
        {
            // fallback
        }

        var cached = await db.StringGetAsync($"config:screen:{screenId}");
        if (!cached.IsNullOrEmpty)
            try
            {
                using var doc = JsonDocument.Parse(cached.ToString());
                var version = doc.RootElement.GetProperty("version").GetInt64();
                var itemsEl = doc.RootElement.GetProperty("items");
                var screensCount = doc.RootElement.GetProperty("screensCount");
                var isStatic = doc.RootElement.GetProperty("isStatic");
                var updatedAt = doc.RootElement.GetProperty("updatedAt");

                if (knownVersion != 0 && knownVersion == version) return StatusCode(304);

                return Ok(new { version, items = itemsEl, screensCount, updatedAt, isStatic });
            }
            catch
            {
            }

        return NoContent();
    }
}