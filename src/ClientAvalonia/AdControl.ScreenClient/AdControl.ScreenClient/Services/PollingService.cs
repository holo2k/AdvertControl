using System.Net;
using System.Text;
using System.Text.Json;
using AdControl.Protos;

namespace AdControl.ScreenClient.Services;

public class ConfigItemDto
{
    public ConfigItemDto(string id, string type, string url, string inlineData, string checksum, long size,
        int durationSeconds, int order)
    {
        Id = id;
        Type = type;
        Url = url;
        InlineData = inlineData;
        Checksum = checksum;
        Size = size;
        DurationSeconds = durationSeconds;
        Order = order;
    }

    public string Id { get; set; }
    public string Type { get; set; }
    public string Url { get; set; }
    public string InlineData { get; set; }
    public string Checksum { get; set; }
    public long Size { get; set; }
    public int DurationSeconds { get; set; }
    public int Order { get; set; }
}

public record ConfigDto(long Version, long UpdatedAt, ConfigItemDto[] Items, bool NotModified = false);

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
                items.Add(new ConfigItemDto(
                    it.GetProperty("id").GetString() ?? "",
                    it.GetProperty("type").GetString() ?? "",
                    it.GetProperty("url").GetString() ?? "",
                    it.GetProperty("inlineData").GetString() ?? "",
                    it.GetProperty("checksum").GetString() ?? "",
                    it.GetProperty("size").GetInt64(),
                    it.GetProperty("durationSeconds").GetInt32(),
                    it.GetProperty("order").GetInt32()
                ));
            var updatedAt = doc.RootElement.TryGetProperty("updatedAt", out var ua) ? ua.GetInt64() : 0;
            return new ConfigDto(ver, updatedAt, items.ToArray());
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

            return new ConfigDto(proto.Version, proto.UpdatedAt, itemsList.ToArray());
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
}