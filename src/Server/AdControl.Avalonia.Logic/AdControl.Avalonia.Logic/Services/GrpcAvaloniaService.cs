using AdControl.Application.Services.Abstractions;
using AdControl.Protos;
using Grpc.Core;

namespace AdControl.Avalonia.Logic.Services;

public class GrpcAvaloniaService : AvaloniaLogicService.AvaloniaLogicServiceBase
{
    private readonly IConfigService _configService;
    private readonly IScreenService _screenService;

    public GrpcAvaloniaService(IConfigService configService, IScreenService screenService)
    {
        _configService = configService;
        _screenService = screenService;
    }

    public override async Task<GetConfigForScreenResponse> GetConfigForScreen(GetConfigForScreenRequest request,
        ServerCallContext context)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.ScreenId))
            return new GetConfigForScreenResponse { Error = "screenId required" };

        try
        {
            var screenId = Guid.Parse(request.ScreenId);
            var screen = await _screenService.GetAsync(screenId, context.CancellationToken);
            if (screen == null)
                return new GetConfigForScreenResponse { Error = "screen not found" };

            var cfg = await _configService.GetConfigForScreenAsync(screenId, context.CancellationToken);
            if (cfg == null)
                return new GetConfigForScreenResponse { Error = "no config" };

            if (request.KnownVersion != 0 && request.KnownVersion == cfg.Version)
                return new GetConfigForScreenResponse { NotModified = true };

            var protoConfig = new ScreenConfigDto
            {
                ConfigId = cfg.Id.ToString(),
                Version = cfg.Version,
                UpdatedAt = new DateTimeOffset(cfg.UpdatedAt).ToUnixTimeMilliseconds()
            };

            foreach (var protoItem in cfg.Items.Select(it => new ConfigItem
                     {
                         Id = it.Id.ToString(),
                         ConfigId = it.ConfigId.ToString(),
                         Url = it.UrlOrData,
                         InlineData = it.UrlOrData.StartsWith("{") ? it.UrlOrData : string.Empty,
                         Checksum = it.Checksum ?? string.Empty,
                         Size = it.Size,
                         DurationSeconds = it.DurationSeconds,
                         Order = it.Order,
                         Type = it.Type.ToUpper() switch
                         {
                             "IMAGE" => ItemType.Image,
                             "VIDEO" => ItemType.Video,
                             "TABLE" => ItemType.Table,
                             "INLINE_JSON" => ItemType.InlineJson,
                             _ => ItemType.Image
                         }
                     }))
                protoConfig.Items.Add(protoItem);

            return new GetConfigForScreenResponse
            {
                Config = protoConfig,
                NotModified = false
            };
        }
        catch (Exception ex)
        {
            return new GetConfigForScreenResponse { Error = $"internal error: {ex.Message}" };
        }
    }
}