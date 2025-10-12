using System.Text.Json;
using AdControl.Gateway.Application.Dtos;
using AdControl.Gateway.Application.Minio;
using AdControl.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Mvc;
using Minio;
using Minio.DataModel.Args;
using Minio.Exceptions;
using StackExchange.Redis;

namespace AdControl.Gateway.Controllers;

[ApiController]
[Route("polling")]
public class PollingController : ControllerBase
{
    private readonly AvaloniaLogicService.AvaloniaLogicServiceClient _avaloniaClient;
    private readonly MinioClient _minio;
    private readonly MinioSettings _minioSettings;
    private readonly IConnectionMultiplexer _redis;

    public PollingController(AvaloniaLogicService.AvaloniaLogicServiceClient avaloniaClient,
        IConnectionMultiplexer redis, MinioClient minio, MinioSettings minioSettings)
    {
        _avaloniaClient = avaloniaClient;
        _redis = redis;
        _minio = minio;
        _minioSettings = minioSettings;
    }

    // POST api/polling/pair/start
    // От экрана. Экран генерирует tempDisplayId и code.
    [HttpPost("pair/start")]
    public async Task<IActionResult> StartPair([FromBody] StartPairDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.TempDisplayId) || string.IsNullOrWhiteSpace(dto.Code))
            return BadRequest(new { error = "TempDisplayId and Code required" });

        var db = _redis.GetDatabase();

        // защита от повторного использования кода: проверяем есть ли уже ключ
        var key = $"pair:{dto.Code}";
        var set = await db.StringSetAsync(key, dto.TempDisplayId,
            TimeSpan.FromMinutes(dto.TtlMinutes > 0 ? dto.TtlMinutes : 10), When.NotExists);
        if (!set)
            return Conflict(new { error = "Code already in use" });

        // можно записать мета-инфо отдельно (необязательно)
        if (!string.IsNullOrEmpty(dto.Info))
            await db.StringSetAsync($"pair:meta:{dto.TempDisplayId}", dto.Info,
                TimeSpan.FromMinutes(dto.TtlMinutes > 0 ? dto.TtlMinutes : 10));

        return Ok(new { ok = true, code = dto.Code, tempDisplayId = dto.TempDisplayId });
    }

    // GET api/polling/pair/status?tempDisplayId=...
    // Экран опрашивает статус привязки
    [HttpGet("pair/status")]
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

    [HttpGet("config")]
    public async Task<IActionResult> GetConfig([FromQuery] string screenId, [FromQuery] long knownVersion = 0)
    {
        if (string.IsNullOrWhiteSpace(screenId)) return BadRequest();

        var db = _redis.GetDatabase();

        // Попробуем gRPC Avalonia.Logic
        try
        {
            var req = new GetConfigForScreenRequest { ScreenId = screenId, KnownVersion = knownVersion };
            var grpcResp = await _avaloniaClient.GetConfigForScreenAsync(req).ResponseAsync;

            if (!string.IsNullOrEmpty(grpcResp.Error))
                return StatusCode(500, new { error = grpcResp.Error });

            if (grpcResp.NotModified)
                return StatusCode(304);

            var cfg = grpcResp.Config;

            // Для каждого item: возможно создать presigned url (если url указывает на minio://bucket/object или s3://)
            var items = new List<object>();
            foreach (var it in cfg.Items)
            {
                var publicUrl = it.Url;
                if (!string.IsNullOrEmpty(it.Url))
                {
                    var presigned = await TryGetPresignedUrlAsync(it.Url);
                    if (!string.IsNullOrEmpty(presigned)) publicUrl = presigned;
                }

                items.Add(new
                {
                    id = it.Id,
                    type = it.Type.ToString(),
                    url = publicUrl,
                    inlineData = it.InlineData,
                    checksum = it.Checksum,
                    size = it.Size,
                    durationSeconds = it.DurationSeconds,
                    order = it.Order
                });
            }

            var cacheObj = new
            {
                version = cfg.Version,
                items,
                updatedAt = cfg.UpdatedAt
            };
            var cacheJson = JsonSerializer.Serialize(cacheObj);
            await db.StringSetAsync($"config:screen:{screenId}", cacheJson);

            return Ok(new { version = cfg.Version, items });
        }
        catch (RpcException)
        {
            // fallback to Redis below
        }
        catch (Exception)
        {
            // fallback to Redis below
        }

        // Fallback: cached config
        var cached = await db.StringGetAsync($"config:screen:{screenId}");
        if (!cached.IsNullOrEmpty)
            try
            {
                using var doc = JsonDocument.Parse(cached.ToString());
                var version = doc.RootElement.GetProperty("version").GetInt64();
                var itemsEl = doc.RootElement.GetProperty("items");
                if (knownVersion != 0 && knownVersion == version) return StatusCode(304);

                // просто вернуть items как JSON
                return Ok(new { version, items = itemsEl });
            }
            catch
            {
                // fallthrough
            }

        return NoContent();
    }

    // Пробуем сформировать presigned url. Поддерживаем форматы:
    // - minio://bucket/object
    // - s3://bucket/object
    // - оставляем оригинал если не поддерживаем
    private async Task<string?> TryGetPresignedUrlAsync(string url)
    {
        if (_minio == null) return url;

        if (url.StartsWith("minio://", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("s3://", StringComparison.OrdinalIgnoreCase))
        {
            // parse bucket and object
            var without = url.Contains("://") ? url.Split("://", 2)[1] : url;
            var idx = without.IndexOf('/');
            if (idx <= 0) return url;
            var bucket = without.Substring(0, idx);
            var obj = without.Substring(idx + 1);

            try
            {
                // PresignedGetObjectAsync signature: (bucketName, objectName, expiry, respHeaders = null)
                var expiry = _minioSettings.PresignExpirySeconds;
                var args = new PresignedGetObjectArgs()
                    .WithBucket(bucket)
                    .WithObject(obj)
                    .WithExpiry(_minioSettings.PresignExpirySeconds);

                var presigned = await _minio.PresignedGetObjectAsync(args);
                return presigned;
            }
            catch (MinioException)
            {
                return url;
            }
        }

        return url;
    }
}