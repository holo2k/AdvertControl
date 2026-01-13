using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Xml.Linq;
using AdControl.Application.Services.Abstractions;
using AdControl.Gateway.Application.Dtos;
using AdControl.Protos;
using Google.Protobuf;
using Grpc.Core;
using Minio.Exceptions;
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

    public override async Task<GetDashboardResponse> GetDashboard(GetDashboardRequest request,
    ServerCallContext context)
    {
        try
        {
            var userIdString = GetUserIdFromMetadata(context);
            if (!Guid.TryParse(userIdString, out var userId))
                throw new UnauthorizedAccessException();

            var result = new GetDashboardResponse() { Success = true, Dashboard = new Dashboard() };
            var screens = (await _screens.GetListByUserIdAsync(userId)).ToList();

            // Основные метрики
            result.Dashboard.ActiveScreens = GetActiveScreensCount(screens);
            result.Dashboard.ConnectedScreens = GetConnectedScreensCount(screens);
            result.Dashboard.ErrorScreens = GetErrorScreensCount(screens);
            result.Dashboard.WaitingScreens = GetWaitingScreensCount(screens);

            // Локации
            result.Dashboard.Locations.AddRange(GetLocations(screens));

            // Действия
            result.Dashboard.Actions.AddRange(GetActions(screens));

            return result;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "GetDashboard failed");
            return new GetDashboardResponse { Success = false, Error = ex.Message };
        }
    }

    public override async Task<CreateScreenResponse> CreateScreen(CreateScreenRequest request,
        ServerCallContext context)
    {
        try
        {
            var userIdString = GetUserIdFromMetadata(context);
            Guid? userId = null;
            if (Guid.TryParse(userIdString, out var g))
                userId = g;
            else throw new UnauthorizedAccessException();
            var created = await _screens.CreateAsync(request.Name, request.Resolution, request.Location, userId);
            return new CreateScreenResponse { Id = created.Id.ToString(), Status = "created" };
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "CreateScreen failed");
            return new CreateScreenResponse { Error = ex.Message };
        }
    }

    public override async Task<DeleteScreenResponse> DeleteScreen(DeleteScreenRequest request,
        ServerCallContext context)
    {
        try
        {
            var userIdString = GetUserIdFromMetadata(context);
            Guid? userId = null;
            if (Guid.TryParse(userIdString, out var g))
                userId = g;
            else
                throw new UnauthorizedAccessException();
            await _screens.DeleteAsync(Guid.Parse(request.Id), CancellationToken.None);
            return new DeleteScreenResponse { Status = "success" };
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "DeleteScreen failed");
            return new DeleteScreenResponse { Error = ex.Message, Status = "error" };
        }
    }

    public override async Task<GetScreenResponse> GetScreen(GetScreenRequest request, ServerCallContext context)
    {
        var userIdString = GetUserIdFromMetadata(context);
        Guid? userId = null;
        if (Guid.TryParse(userIdString, out var g))
            userId = g;
        else throw new UnauthorizedAccessException();

        if (!Guid.TryParse(request.Id, out var id))
            return new GetScreenResponse();

        var s = await _screens.GetAsync(id);
        if (s == null) return new GetScreenResponse();
        if (s.UserId != userId)
            throw new UnauthorizedAccessException();
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

        var types = s.ScreenConfigs
            .Select(sc => sc.Config)
            .SelectMany(c => c.Items)
            .Select(i => i.Type);


        var activeScreenConfig = s.ScreenConfigs
              .FirstOrDefault(sc => sc.IsActive);

        var protoConfig = new Config();

        if (activeScreenConfig is not null)
        {
            var config = activeScreenConfig.Config;
            if (config is not null)
            {
                protoConfig = new Config
                {
                    Id = config.Id.ToString(),
                    CreatedAt = DateTimeToUnixMs(config.CreatedAt),
                    UpdatedAt = DateTimeToUnixMs(config.UpdatedAt),
                    UserId = config.UserId.ToString(),
                    Name = config.Name,
                    ScreensCount = config.ScreensCount,
                    IsStatic = config.IsStatic,
                    Version = config.Version,
                };

                foreach (var it in config.Items)
                    protoConfig.Items.Add(new Protos.ConfigItem
                    {
                        Id = it.Id.ToString(),
                        ConfigId = it.ConfigId.ToString(),
                        Type = Enum.TryParse<ItemType>(it.Type, true, out var t)
                            ? t
                            : ItemType.Image,
                        Url = it.Url,
                        InlineData = it.InlineData,
                        Checksum = it.Checksum ?? "",
                        Size = it.Size,
                        DurationSeconds = it.DurationSeconds,
                        Order = it.Order
                    });
            }
        }

        return new GetScreenResponse { Screen = proto, Config = protoConfig };
    }

    public override async Task<ListUserScreensResponse> GetListUserScreens(ListUserScreensRequest request,
        ServerCallContext context)
    {
        var userIdString = GetUserIdFromMetadata(context);
        Guid? userId = null;
        if (Guid.TryParse(userIdString, out var g))
            userId = g;
        else throw new UnauthorizedAccessException();

        var screenListByUserId = await _screens.GetListByUserIdAsync(Guid.Parse(request.UserId));
        if (screenListByUserId.Any(s => s.UserId != userId))
            throw new UnauthorizedAccessException();

        var resp = new ListUserScreensResponse();
        foreach (var screen in screenListByUserId)
        {
            resp.Screens.Add(new Screen
            {
                Id = screen.Id.ToString(),
                UserId = screen.UserId?.ToString() ?? "",
                Name = screen.Name,
                Resolution = screen.Resolution,
                Location = screen.Location,
                LastHeartbeatAt = screen.LastHeartbeatAt.HasValue ? DateTimeToUnixMs(screen.LastHeartbeatAt.Value) : 0,
                PairedAt = screen.PairedAt.HasValue ? DateTimeToUnixMs(screen.PairedAt.Value) : 0,
                CreatedAt = DateTimeToUnixMs(screen.CreatedAt),
                UpdatedAt = DateTimeToUnixMs(screen.UpdatedAt)
            });
        }

        resp.Total = resp.Screens.Count;

        return resp;
    }

    public override async Task<ListScreensResponse> ListScreens(ListScreensRequest request, ServerCallContext context)
    {
        var userIdString = GetUserIdFromMetadata(context);
        Guid? userId = null;
        if (Guid.TryParse(userIdString, out var g))
            userId = g;
        else throw new UnauthorizedAccessException();

        var (items, total) = await _screens.ListAsync(request.FilterName, request.Limit, request.Offset);

        if (items.Any(s => s.UserId != userId))
            throw new UnauthorizedAccessException();

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
            if (Guid.TryParse(userIdString, out var g))
                userId = g;
            else throw new UnauthorizedAccessException();

            var items = request.Items.Select(i => new ConfigItem
            {
                Id = string.IsNullOrEmpty(i.Id) ? Guid.NewGuid() : Guid.Parse(i.Id),
                ConfigId = Guid.Empty, // will be set in service
                Type = i.Type.ToString(),
                Url = i.Url,
                InlineData = i.InlineData,
                Checksum = i.Checksum,
                Size = i.Size,
                DurationSeconds = i.DurationSeconds,
                Order = i.Order
            }).ToList();

            var cfg = await _configs.CreateAsync(request.Name, userId, items, request.ScreensCount, request.IsStatic);
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
        var userIdString = GetUserIdFromMetadata(context);
        Guid? userId = null;
        if (Guid.TryParse(userIdString, out var g))
            userId = g;
        else throw new UnauthorizedAccessException();

        if (!Guid.TryParse(request.Id, out var id)) return new GetConfigResponse();
        var cfg = await _configs.GetAsync(id);
        if (cfg == null) return new GetConfigResponse();


        if (cfg.UserId != userId)
            throw new UnauthorizedAccessException();

        var proto = new Config
        {
            Id = cfg.Id.ToString(), 
            UserId = cfg.UserId?.ToString() ?? "",
            CreatedAt = DateTimeToUnixMs(cfg.CreatedAt),
            ScreensCount = cfg.ScreensCount, 
            IsStatic = cfg.IsStatic,
            UpdatedAt =  DateTimeToUnixMs(cfg.UpdatedAt),
            Name = cfg.Name,
        };

        foreach (var it in cfg.Items)
            proto.Items.Add(new Protos.ConfigItem
            {
                Id = it.Id.ToString(),
                ConfigId = it.ConfigId.ToString(),
                Type = Enum.TryParse<ItemType>(it.Type, true, out var t)
                    ? t
                    : ItemType.Image,
                Url = it.Url,
                InlineData = it.InlineData,
                Checksum = it.Checksum ?? "",
                Size = it.Size,
                DurationSeconds = it.DurationSeconds,
                Order = it.Order
            });
        return new GetConfigResponse { Config = proto };
    }
    
    public override async Task<GetConfigsResponse> GetConfigs(GetConfigsRequest request, ServerCallContext context)
    {
        var userIdString = GetUserIdFromMetadata(context);
        if (!Guid.TryParse(userIdString, out var userId))
            throw new UnauthorizedAccessException();

        var configs = await _configs.GetUserConfigs(userId);
        var response = new GetConfigsResponse();

        if (configs == null)
            return response;

        foreach (var cfg in configs)
        {
            var proto = new Config
            {
                Id = cfg.Id.ToString(),
                Name = cfg.Name,
                UpdatedAt = DateTimeToUnixMs(cfg.UpdatedAt),
                UserId = cfg.UserId?.ToString() ?? "",
                CreatedAt = DateTimeToUnixMs(cfg.CreatedAt),
                Version = cfg.Version,
                ScreensCount = cfg.ScreensCount,
                IsStatic = cfg.IsStatic,
            };

            foreach (var it in cfg.Items)
            {
                proto.Items.Add(new Protos.ConfigItem
                {
                    Id = it.Id.ToString(),
                    ConfigId = it.ConfigId.ToString(),
                    Type = Enum.TryParse<ItemType>(it.Type, true, out var t)
                        ? t
                        : ItemType.Image,
                    Url = it.Url,
                    InlineData = it.InlineData,
                    Checksum = it.Checksum ?? "",
                    Size = it.Size,
                    DurationSeconds = it.DurationSeconds,
                    Order = it.Order
                });
            }

            response.Configs.Add(proto);
        }

        return response;
    }

    public override async Task<UpdateScreenFieldsResponse> UpdateScreenFields(UpdateScreenFieldsRequest request,
        ServerCallContext context)
    {
        var userIdString = GetUserIdFromMetadata(context);
        Guid? userId = null;
        if (Guid.TryParse(userIdString, out var g))
            userId = g;
        else throw new UnauthorizedAccessException();
        try
        {
            var guidId = Guid.Parse(request.Id);
            var screen = await _screens.GetAsync(guidId);
            if (screen == null) throw new ArgumentException("screen not found");
            
            var name = request.Name;
            var resolution = request.Resolution;
            var location = request.Location;
            if (name is not null && !string.IsNullOrEmpty(name))
            {
                screen.Name = name;
            }
            if (resolution is not null && !string.IsNullOrEmpty(resolution))
            {
                screen.Resolution = resolution;
            }
            if (location is not null && !string.IsNullOrEmpty(location))
            {
                screen.Location = location;
            }
            var newScreen = await _screens.UpdateAsync(screen);
            return new UpdateScreenFieldsResponse
            {
                Screen = new Screen
                {
                    Id = newScreen.Id.ToString(),
                    UserId = newScreen.UserId?.ToString() ?? "",
                    Name = newScreen.Name,
                    Resolution = newScreen.Resolution,
                    Location = newScreen.Location,
                    LastHeartbeatAt = newScreen.LastHeartbeatAt.HasValue ? DateTimeToUnixMs(newScreen.LastHeartbeatAt.Value) : 0,
                    PairedAt = newScreen.PairedAt.HasValue ? DateTimeToUnixMs(newScreen.PairedAt.Value) : 0,
                    CreatedAt = DateTimeToUnixMs(newScreen.CreatedAt),
                    UpdatedAt = DateTimeToUnixMs(newScreen.UpdatedAt)
                },
            };
        }
        
        catch (Exception ex)
        {
            _log.LogError(ex, "UpdateScreenFields failed");
            return new UpdateScreenFieldsResponse();
        }
    }

    public override async Task<UpdateConfigResponse> UpdateConfigFields(UpdateConfigRequest request,
       ServerCallContext context)
    {
        var userIdString = GetUserIdFromMetadata(context);
        Guid? userId = null;
        if (Guid.TryParse(userIdString, out var g))
            userId = g;
        else
            throw new UnauthorizedAccessException();
        try
        {
            var guidId = Guid.Parse(request.Id);
            var config = await _configs.GetAsync(guidId);
            if (config == null)
                throw new ArgumentException("config not found");

            var name = request.Name;
            var screensCount = request.ScreensCount;
            var isStatic = request.IsStatic;
            
            if (name is not null && !string.IsNullOrEmpty(name))
            {
                config.Name = name;
            }
            if (screensCount > 0)
            {
                config.ScreensCount = screensCount;
            }

            config.IsStatic = isStatic;
            
            var newConfig = await _configs.UpdateAsync(config);
            return new UpdateConfigResponse
            {
                Success = true,
                Error = ""
            };
        }

        catch (Exception ex)
        {
            _log.LogError(ex, "UpdateConfigFields failed");
            return new UpdateConfigResponse
            {
                Success = false,
                Error = $"UpdateConfigFields failed. \n Detailed: {ex.Message}"
            };
        }
    }
    
    public override async Task<UpdateConfigResponseV2> UpdateConfig(Config request,
        ServerCallContext context)
    {
        var userIdString = GetUserIdFromMetadata(context);
        Guid? userId = null;
        if (Guid.TryParse(userIdString, out var g))
            userId = g;
        else
            throw new UnauthorizedAccessException();
        try
        {
            var guidId = Guid.Parse(request.Id);
            var config = await _configs.GetAsync(guidId);
            if (config == null)
                throw new ArgumentException("config not found");

            config.IsStatic = request.IsStatic;
            config.UserId = userId;
            config.CreatedAt = new DateTime(request.CreatedAt);
            config.Items = request.Items.Select(i => new ConfigItem
            {
                Id = string.IsNullOrEmpty(i.Id) ? Guid.NewGuid() : Guid.Parse(i.Id),
                ConfigId = Guid.Empty, // will be set in service
                Type = i.Type.ToString(),
                Url = i.Url,
                InlineData = i.InlineData,
                Checksum = i.Checksum,
                Size = i.Size,
                DurationSeconds = i.DurationSeconds,
                Order = i.Order
            }).ToList();
            config.Version = request.Version;
            config.Name = request.Name;
            config.ScreensCount = request.ScreensCount;
            config.UpdatedAt = new DateTime(request.UpdatedAt);
            
            var newConfig = await _configs.UpdateAsync(config);
            var protoItems = newConfig.Items.Select(i => new AdControl.Protos.ConfigItem
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
            
            return new UpdateConfigResponseV2
            {
                Success = true,
                Error = "",
                Config = new Config
                {
                    CreatedAt = DateTimeToUnixMs(newConfig.CreatedAt),
                    Name = newConfig.Name,
                    ScreensCount = newConfig.ScreensCount,
                    IsStatic = newConfig.IsStatic,
                    UpdatedAt = DateTimeToUnixMs(newConfig.UpdatedAt),
                    UserId = newConfig.UserId?.ToString() ?? "",
                    Version = newConfig.Version,
                    Items = { protoItems }
                }
            };
        }

        catch (Exception ex)
        {
            _log.LogError(ex, "UpdateConfigFields failed");
            return new UpdateConfigResponseV2
            {
                Success = false,
                Error = $"UpdateConfigFields failed. \n Detailed: {ex.Message}",
            };
        }
    }

    public override async Task<RemoveItemResponse> RemoveConfigItem(RemoveItemRequest request, ServerCallContext context)
    {
        var userIdString = GetUserIdFromMetadata(context);
        Guid? userId = null;
        if (Guid.TryParse(userIdString, out var g))
            userId = g;
        else throw new UnauthorizedAccessException();

        try
        {
            var guidId = Guid.Parse(request.Id);
            var guidItemId = Guid.Parse(request.ItemId);
            var deleted = await _configs.DeleteConfigItemAsync(guidId, guidItemId);
            return new RemoveItemResponse
            {
                Success = deleted,
            };
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "RemoveConfigItem failed");
            return new RemoveItemResponse();
        }
    }

    public override async Task<AddItemsResponse> AddConfigItems(AddItemsRequest request, ServerCallContext context)
    {
        var userIdString = GetUserIdFromMetadata(context);
        Guid? userId = null;
        if (Guid.TryParse(userIdString, out var g))
            userId = g;
        else throw new UnauthorizedAccessException();

        try
        {
            if (!Guid.TryParse(request.Id, out var configId)) throw new ValidationException("Wrong config id");
            var protoItems = request.Items;
            var items = protoItems.Select(it => new ConfigItem
                {
                    Id = string.IsNullOrEmpty(it.Id) ? Guid.NewGuid() : Guid.Parse(it.Id),
                    ConfigId = Guid.Parse(it.ConfigId),
                    Type = it.Type.ToString(),
                    Url = it.Url,
                    InlineData = it.InlineData,
                    Checksum = it.Checksum ?? "",
                    Size = it.Size,
                    DurationSeconds = it.DurationSeconds,
                    Order = it.Order
                })
                .ToList();

            var cfg = await _configs.AddItems(configId, items, context.CancellationToken);
            var protoCfg = new Config
            {
                Id = cfg.Id.ToString(), UserId = cfg.UserId?.ToString() ?? "",
                UpdatedAt = DateTimeToUnixMs(cfg.UpdatedAt), 
                Items = { protoItems }
            };
            var response = new AddItemsResponse
            {
                Config = protoCfg
            };
            return response;
        }
        catch (Exception ex)
        {
            _log.LogError(ex, "Add items failed");
            return new AddItemsResponse();
        }
    }

    public override async Task<AssignConfigResponse> AssignConfigToScreen(AssignConfigRequest request,
        ServerCallContext context)
    {
        var userIdString = GetUserIdFromMetadata(context);
        Guid? userId = null;
        if (Guid.TryParse(userIdString, out var g))
            userId = g;
        else throw new UnauthorizedAccessException();

        try
        {
            if (!Guid.TryParse(request.ConfigId, out var configId)) throw new ValidationException("Wrong config id");
            var cfg = await _configs.GetAsync(configId, context.CancellationToken);
            if (cfg == null) throw new NullReferenceException("Config does not exist");

            if (!Guid.TryParse(request.ScreenId, out var id)) throw new ValidationException("Wrong screen id");
            var s = await _screens.GetAsync(id);
            if (s == null) throw new NullReferenceException("Screen does not exist");

            if (cfg.UserId != userId || s.UserId != userId) throw new UnauthorizedAccessException();

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

    private int GetActiveScreensCount(List<Domain.Models.Screen> screens)
        => screens.Count(IsActive);

    private int GetConnectedScreensCount(List<Domain.Models.Screen> screens)
        => screens.Count(IsConnected);

    private int GetErrorScreensCount(List<Domain.Models.Screen> screens)
        => screens.Count(IsError);

    private int GetWaitingScreensCount(List<Domain.Models.Screen> screens)
        => screens.Count(IsWaiting);

    private List<ScreenLocations> GetLocations(List<Domain.Models.Screen> screens)
    {
        return screens
            .GroupBy(s => s.Location)
            .Select(g => new ScreenLocations
            {
                ScreenLocation = g.Key,
                Count = g.Count()
            })
            .ToList();
    }

    private List<Protos.Action> GetActions(List<Domain.Models.Screen> screens)
    {
        return screens
            .Select(s => new Protos.Action
            {
                ScreenId = s.Id.ToString(),
                Screen = s.Name,
                Action_ = BuildActionStatus(s),
                LastUpdate = s.LastHeartbeatAt.HasValue && s.LastHeartbeatAt.Value.Year < 2025 ? 
                             s.LastHeartbeatAt.Value.ToString() : s.UpdatedAt.ToString(),
                Status = IsSuccess(s)
            })
            .ToList();
    }

    private string BuildActionStatus(Domain.Models.Screen s)
    {
        var isSuccess = IsSuccess(s);

        // Добавлен экран
        if (s.PairedAt.HasValue &&
            s.PairedAt.Value.Year < 2025)
            return "Добавлен экран";

        // Синхронизирован
        if (s.PairedAt.HasValue &&
            s.PairedAt.Value.Year >= 2025 &&
            s.LastHeartbeatAt.HasValue &&
            s.LastHeartbeatAt.Value.Year < 2025)
            return "Синхронизирован";

        // Изменена конфигурация
        var lastConfigUpdate = s.ScreenConfigs
            .OrderByDescending(c => c.Config.UpdatedAt)
            .FirstOrDefault()?.Config.UpdatedAt;

        if (lastConfigUpdate.HasValue &&
            (DateTime.UtcNow - lastConfigUpdate.Value).TotalSeconds < 5)
            return "Изменена конфигурация";

        if (!isSuccess)
            return "Соединение потеряно";

        // Контент обновлён
        if (s.LastHeartbeatAt.HasValue)
        {
            var diff = (DateTime.UtcNow - s.LastHeartbeatAt.Value).TotalSeconds;

            if (diff <= 5)
                return "Контент обновлен";
        }

        return "";
    }

    private bool IsSuccess(Domain.Models.Screen s)
    {
        return s.LastHeartbeatAt is { } ts &&
               DateTime.UtcNow - ts <= TimeSpan.FromMinutes(3);
    }

    private bool IsError(Domain.Models.Screen s)
    {
        return !IsSuccess(s);
    }

    private bool IsWaiting(Domain.Models.Screen s)
    {
        return s.PairedAt.HasValue &&
               s.PairedAt.Value.Year >= 2025 &&
               s.ScreenConfigs.FirstOrDefault(sc => sc.ScreenId == s.Id)?
                    .Config.Items.Count == 0;
    }

    private bool IsConnected(Domain.Models.Screen s)
    {
        return s.PairedAt.HasValue &&
               s.PairedAt.Value.Year >= 2025 &&
               IsSuccess(s) &&
               !IsWaiting(s);
    }

    private bool IsActive(Domain.Models.Screen s)
    {
        return IsSuccess(s) &&
               !IsConnected(s) &&
               !IsWaiting(s);
    }
}