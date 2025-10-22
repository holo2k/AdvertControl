using AdControl.Gateway.Application.Dtos;
using AdControl.Protos;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace AdControl.Gateway.Controllers;

[ApiController]
[Route("api/screen")]
public class ScreenController : ControllerBase
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ScreenService.ScreenServiceClient _screenClient;

    public ScreenController(ScreenService.ScreenServiceClient screenClient, IConnectionMultiplexer redis)
    {
        _screenClient = screenClient;
        _redis = redis;
    }

    // POST api/screen
    [HttpPost]
    [Authorize]
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

    // GET api/screen/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(string id)
    {
        var req = new GetScreenRequest { Id = id };
        var metadata = BuildAuthMetadata(HttpContext);
        var resp = await _screenClient.GetScreenAsync(req, metadata).ResponseAsync;
        if (resp.Screen == null || string.IsNullOrEmpty(resp.Screen.Id))
            return NotFound();
        return Ok(resp.Screen);
    }

    // GET api/screen?filterName=...&limit=50&offset=0
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> List([FromQuery] string? filterName, [FromQuery] int limit = 50,
        [FromQuery] int offset = 0)
    {
        var req = new ListScreensRequest { FilterName = filterName ?? "", Limit = limit, Offset = offset };
        var resp = await _screenClient.ListScreensAsync(req, BuildAuthMetadata(HttpContext)).ResponseAsync;
        return Ok(new { items = resp.Screens, total = resp.Total });
    }

    // DELETE api/screen/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(string id)
    {
        var metadata = BuildAuthMetadata(HttpContext);
        var call = _screenClient.DeleteScreenAsync(new DeleteScreenRequest { Id = id }, metadata);
        var resp = await call.ResponseAsync;
        return StatusCode(501, "Delete not implemented in proto");
    }


    private Metadata BuildAuthMetadata(HttpContext http)
    {
        var metadata = new Metadata();
        if (http.Request.Headers.TryGetValue("Authorization", out var auth))
            metadata.Add("Authorization", auth.ToString());
        return metadata;
    }

    // POST api/screen/pair/confirm
    // От веба (Authenticated). Подтверждает код, создаёт Screen в ScreenService и связывает.
    [HttpPost("pair/confirm")]
    [Authorize]
    public async Task<IActionResult> ConfirmPair([FromBody] ConfirmPairDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Code)) return BadRequest();
        var db = _redis.GetDatabase();

        var key = $"pair:{dto.Code}";
        var tempId = await db.StringGetAsync(key);
        if (tempId.IsNullOrEmpty) return NotFound(new { error = "code not found or expired" });

        // Создаём экран в downstream gRPC сервисе от имени текущего пользователя (пробрасываем Authorization)
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

        // cleanup pairing code
        await db.KeyDeleteAsync(key);
        await db.KeyDeleteAsync($"pair:meta:{tempId}");

        return Ok(new { id = resp.Id, status = resp.Status });
    }
}