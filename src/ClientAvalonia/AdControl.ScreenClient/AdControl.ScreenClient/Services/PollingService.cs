using System.Net;
using System.Text;
using System.Text.Json;
using AdControl.Protos;

namespace AdControl.ScreenClient.Services;

public class ConfigItemDto : IEquatable<ConfigItemDto>
{
    public ConfigItemDto(string id, string type, string urlFileName, string inlineData, string checksum, long size,
        int durationSeconds, int order)
    {
        Id = id;
        Type = type;
        Url = urlFileName; // хранит только имя файла, напр. logo.png
        InlineData = inlineData;
        Checksum = checksum;
        Size = size;
        DurationSeconds = durationSeconds;
        Order = order;
    }

    public string Id { get; set; }
    public string Type { get; set; }
    public string Url { get; set; } // file name only
    public string InlineData { get; set; }
    public string Checksum { get; set; }
    public long Size { get; set; }
    public int DurationSeconds { get; set; }
    public int Order { get; set; }

    // фабричный метод для безопасного парсинга JsonElement
    public static ConfigItemDto FromJsonElement(JsonElement el)
    {
        string GetStringOrEmpty(string name)
        {
            return el.TryGetProperty(name, out var prop) && prop.ValueKind != JsonValueKind.Null
                ? prop.GetString() ?? ""
                : "";
        }

        long GetLongOrDefault(string name)
        {
            return el.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.Number && prop.TryGetInt64(out var v)
                ? v
                : 0L;
        }

        int GetIntOrDefault(string name)
        {
            return el.TryGetProperty(name, out var prop) && prop.ValueKind == JsonValueKind.Number && prop.TryGetInt32(out var v)
                ? v
                : 0;
        }

        var rawUrl = GetStringOrEmpty("url");
        var fileName = string.IsNullOrEmpty(rawUrl) ? "" : Path.GetFileName(rawUrl); // NORMALIZE -> file name only

        return new ConfigItemDto(
            id: GetStringOrEmpty("id"),
            type: GetStringOrEmpty("type"),
            urlFileName: fileName,
            inlineData: GetStringOrEmpty("inlineData"),
            checksum: GetStringOrEmpty("checksum"),
            size: GetLongOrDefault("size"),
            durationSeconds: GetIntOrDefault("durationSeconds"),
            order: GetIntOrDefault("order")
        );
    }

    public override bool Equals(object? obj)
    {
        if (obj is null)
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((ConfigItemDto)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Type, Url, InlineData, Checksum, Size, DurationSeconds, Order);
    }

    public bool Equals(ConfigItemDto? other)
    {
        if (other is null)
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return Id == other.Id && Type == other.Type && Url == other.Url && InlineData == other.InlineData
               && Checksum == other.Checksum && Size == other.Size && DurationSeconds == other.DurationSeconds
               && Order == other.Order;
    }
}

public record ConfigDto(long Version, long UpdatedAt, ConfigItemDto[] Items, bool NotModified = false, int WindowCount = 0, bool isStatic = false);

public class PollingService
{
    private readonly AvaloniaLogicService.AvaloniaLogicServiceClient _avaloniaClient;
    private readonly IHttpClientFactory _http;

    public PollingService(IHttpClientFactory http, AvaloniaLogicService.AvaloniaLogicServiceClient avaloniaClient)
    {
        _http = http;
        _avaloniaClient = avaloniaClient;
    }

    public async Task<ConfigDto?> GetConfigAsync(string screenId, long knownVersion = 0)
    {
        var client = _http.CreateClient("gateway");
        try
        {
            var url = $"/api/polling/config?screenId={Uri.EscapeDataString(screenId)}&knownVersion={knownVersion}";
            using var resp = await client.GetAsync(url);
            if (resp.StatusCode == HttpStatusCode.NotModified)
                return new ConfigDto(knownVersion, 0, Array.Empty<ConfigItemDto>(), true);
            resp.EnsureSuccessStatusCode();
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var ver = doc.RootElement.GetProperty("version").GetInt64();
            var itemsEl = doc.RootElement.GetProperty("items");
            var items = new List<ConfigItemDto>();
            foreach (var it in itemsEl.EnumerateArray())
                items.Add(ConfigItemDto.FromJsonElement(it));
            var updatedAt = doc.RootElement.TryGetProperty("updatedAt", out var ua) ? ua.GetInt64() : 0;
            var screensCount = doc.RootElement.TryGetProperty("screensCount", out var sc) ? sc.GetInt32() : 0;
            var isStatic = doc.RootElement.TryGetProperty("isStatic", out var iss) && iss.GetBoolean();
            var notModified = knownVersion == ver;

            return new ConfigDto(
                ver,
                updatedAt,
                items
                    .OrderBy(x => x.Order)
                    .ToArray(),
                notModified,
                screensCount,
                isStatic
            );
        }
        catch (HttpRequestException)
        {
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception)
        {
        }

        // fallback gRPC to Avalonia.Logic
        try
        {
            var grpcReq = new GetConfigForScreenRequest { ScreenId = screenId, KnownVersion = knownVersion };
            var rpc = await _avaloniaClient.GetConfigForScreenAsync(grpcReq);
            if (!string.IsNullOrEmpty(rpc.Error)) throw new Exception(rpc.Error);
            if (rpc.NotModified) return new ConfigDto(knownVersion, 0, Array.Empty<ConfigItemDto>(), true);

            var proto = rpc.Config;
            var itemsList = new List<ConfigItemDto>();
            foreach (var it in proto.Items)
                itemsList.Add(new ConfigItemDto(
                    it.Id,
                    it.Type.ToString(),
                    it.Url,
                    it.InlineData,
                    it.Checksum,
                    it.Size,
                    it.DurationSeconds,
                    it.Order
                ));

            var notModified = proto.Version == knownVersion;

            return new ConfigDto(
                proto.Version,
                proto.UpdatedAt,
                itemsList
                    .OrderBy(x => x.Order)
                    .ToArray(),
                notModified,
                proto.ScreensCount,
                proto.IsStatic
            );
        }
        catch (Exception)
        {
            return null;
        }
    }

    // POST /api/polling/pair/start
    public async Task<bool> StartPairAsync(string tempDisplayId, string code, int ttlMinutes = 10, string? info = null)
    {
        var client = _http.CreateClient("gateway");
        var payload = new
        {
            TempDisplayId = tempDisplayId,
            Code = code,
            TtlMinutes = ttlMinutes,
            Info = info
        };
        var json = JsonSerializer.Serialize(payload);
        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        try
        {
            using var resp = await client.PostAsync("/api/polling/pair/start", content);
            return resp.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // GET /api/polling/pair/status?tempDisplayId=...
    public async Task<(bool assigned, string? screenId)> CheckPairStatusAsync(string tempDisplayId)
    {
        var client = _http.CreateClient("gateway");
        try
        {
            var url = $"/api/polling/pair/status?tempDisplayId={Uri.EscapeDataString(tempDisplayId)}";
            using var resp = await client.GetAsync(url);
            if (!resp.IsSuccessStatusCode) return (false, null);
            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var assigned = doc.RootElement.GetProperty("assigned").GetBoolean();
            var screenId = doc.RootElement.TryGetProperty("screenId", out var sid) &&
                           sid.ValueKind != JsonValueKind.Null
                ? sid.GetString()
                : null;
            return (assigned, screenId);
        }
        catch
        {
            return (false, null);
        }
    }

    public async Task<bool?> IsScreenExistAsync(string screenId)
    {
        var client = _http.CreateClient("gateway");
        try
        {
            var url = $"/api/polling/is-assigned/{Uri.EscapeDataString(screenId)}";
            var resp = await client.GetAsync(url);

            var content = await resp.Content.ReadAsStringAsync();

            return bool.Parse(content);
        }
        catch (HttpRequestException)
        {
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception)
        {
        }

        // fallback gRPC to Avalonia.Logic
        try
        {
            var grpcReq = new IsScreenExistRequest { ScreenId = screenId };
            var rpc = await _avaloniaClient.IsScreenExistAsync(grpcReq);
            return rpc.IsExist;
        }
        catch (Exception)
        {
            return null;
        }
    }
}