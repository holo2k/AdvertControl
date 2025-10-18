using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AdControl.Application.Services.Abstractions;
using AdControl.Protos;
using Grpc.Core;
using ConfigItem = AdControl.Domain.Models.ConfigItem;

namespace AdControl.Web.Services;

public class GrpcScreenService : ScreenService.ScreenServiceBase
{
    private readonly IConfigService _configs;
    private readonly ILogger<GrpcScreenService> _log;
    private readonly IScreenService _screens;

    public GrpcScreenService(IScreenService screens, IConfigService configs, ILogger<GrpcScreenService> log)
    {
        _screens = screens;
        _configs = configs;
        _log = log;
    }

    public override async Task<CreateScreenResponse> CreateScreen(CreateScreenRequest request,
        ServerCallContext context)
    {
        try
        {
            var userIdString = GetUserIdFromMetadata(context);
            Guid? userId = null;
            if (Guid.TryParse(userIdString, out var g)) userId = g;
            var created = await _screens.CreateAsync(request.Name, request.Resolution, request.Location, userId);
            return new CreateScreenResponse { Id = created.Id.ToString(), Status = "created" };
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "CreateScreen failed");
            return new CreateScreenResponse { Error = ex.Message };
        }
    }

    public override async Task<GetScreenResponse> GetScreen(GetScreenRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id))
            return new GetScreenResponse();

        var s = await _screens.GetAsync(id);
        if (s == null) return new GetScreenResponse();

        var proto = new Screen
        {
            Id = s.Id.ToString(),
            UserId = s.UserId?.ToString() ?? "",
            Name = s.Name,
            Resolution = s.Resolution,
            Location = s.Location,
            LastHeartbeatAt = s.LastHeartbeatAt.HasValue ? DateTimeToUnixMs(s.LastHeartbeatAt.Value) : 0,
            PairedAt = s.PairedAt.HasValue ? DateTimeToUnixMs(s.PairedAt.Value) : 0,
            CreatedAt = DateTimeToUnixMs(s.CreatedAt),
            UpdatedAt = DateTimeToUnixMs(s.UpdatedAt)
        };

        return new GetScreenResponse { Screen = proto };
    }

    public override async Task<ListScreensResponse> ListScreens(ListScreensRequest request, ServerCallContext context)
    {
        var (items, total) = await _screens.ListAsync(request.FilterName, request.Limit, request.Offset);
        var resp = new ListScreensResponse();
        foreach (var s in items)
            resp.Screens.Add(new Screen
            {
                Id = s.Id.ToString(),
                UserId = s.UserId?.ToString() ?? "",
                Name = s.Name,
                Resolution = s.Resolution,
                Location = s.Location,
                LastHeartbeatAt = s.LastHeartbeatAt.HasValue ? DateTimeToUnixMs(s.LastHeartbeatAt.Value) : 0,
                PairedAt = s.PairedAt.HasValue ? DateTimeToUnixMs(s.PairedAt.Value) : 0,
                CreatedAt = DateTimeToUnixMs(s.CreatedAt),
                UpdatedAt = DateTimeToUnixMs(s.UpdatedAt)
            });
        resp.Total = total;
        return resp;
    }

    public override async Task<CreateConfigResponse> CreateConfig(CreateConfigRequest request,
        ServerCallContext context)
    {
        try
        {
            var userIdString = GetUserIdFromMetadata(context);
            Guid? userId = null;
            if (Guid.TryParse(userIdString, out var g)) userId = g;

            var items = request.Items.Select(i => new ConfigItem
            {
                Id = string.IsNullOrEmpty(i.Id) ? Guid.NewGuid() : Guid.Parse(i.Id),
                ConfigId = Guid.Empty, // will be set in service
                Type = i.Type.ToString(),
                UrlOrData = string.IsNullOrEmpty(i.Url) ? i.InlineData ?? "" : i.Url,
                Checksum = i.Checksum,
                Size = i.Size,
                DurationSeconds = i.DurationSeconds,
                Order = i.Order
            }).ToList();

            var cfg = await _configs.CreateAsync(userId, items);
            return new CreateConfigResponse { Id = cfg.Id.ToString(), Status = "created" };
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "CreateConfig failed");
            return new CreateConfigResponse { Error = ex.Message };
        }
    }

    public override async Task<GetConfigResponse> GetConfig(GetConfigRequest request, ServerCallContext context)
    {
        if (!Guid.TryParse(request.Id, out var id)) return new GetConfigResponse();
        var cfg = await _configs.GetAsync(id);
        if (cfg == null) return new GetConfigResponse();

        var proto = new Config
        {
            Id = cfg.Id.ToString(), UserId = cfg.UserId?.ToString() ?? "", CreatedAt = DateTimeToUnixMs(cfg.CreatedAt)
        };
        foreach (var it in cfg.Items)
            proto.Items.Add(new Protos.ConfigItem
            {
                Id = it.Id.ToString(),
                ConfigId = it.ConfigId.ToString(),
                Type = Enum.TryParse<ItemType>(it.Type, true, out var t)
                    ? t
                    : ItemType.Image,
                Url = it.UrlOrData,
                InlineData = it.UrlOrData,
                Checksum = it.Checksum ?? "",
                Size = it.Size,
                DurationSeconds = it.DurationSeconds,
                Order = it.Order
            });
        return new GetConfigResponse { Config = proto };
    }

    public override async Task<AssignConfigResponse> AssignConfigToScreen(AssignConfigRequest request,
        ServerCallContext context)
    {
        try
        {
            if (!Guid.TryParse(request.ScreenId, out var sid) || !Guid.TryParse(request.ConfigId, out var cid))
                return new AssignConfigResponse { Error = "invalid ids" };

            await _configs.AssignToScreenAsync(sid, cid, request.IsActive);
            return new AssignConfigResponse { Id = Guid.NewGuid().ToString(), Status = "assigned" };
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Assign failed");
            return new AssignConfigResponse { Error = ex.Message };
        }
    }

    private static long DateTimeToUnixMs(DateTime dt)
    {
        return new DateTimeOffset(dt.ToUniversalTime()).ToUnixTimeMilliseconds();
    }

    private static string? GetUserIdFromMetadata(ServerCallContext context)
    {
        var authEntry =
            context.RequestHeaders.FirstOrDefault(h => h.Key == "authorization" || h.Key == "authorization-bin");
        if (authEntry == null) return null;

        var auth = authEntry.Value;
        if (string.IsNullOrEmpty(auth)) return null;

        // "Bearer <token>"
        var token = auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) ? auth.Substring(7) : auth;

        var handler = new JwtSecurityTokenHandler();
        if (!handler.CanReadToken(token)) return null;
        var jwt = handler.ReadJwtToken(token);

        var sub = jwt.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;
        return sub;
    }
}