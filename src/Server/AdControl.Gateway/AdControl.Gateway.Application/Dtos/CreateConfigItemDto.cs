namespace AdControl.Gateway.Application.Dtos;

public class CreateConfigItemDto
{
    public int DurationSeconds;
    public int Order;

    public CreateConfigItemDto(int durationSeconds, int order, string? id, string? type, string? urlOrData, string url,
        string inlineData, string checksum)
    {
        DurationSeconds = durationSeconds;
        Order = order;
        Id = id;
        Type = type;
        UrlOrData = urlOrData;
        Url = url;
        InlineData = inlineData;
        Checksum = checksum;
    }

    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? UrlOrData { get; set; }
    public string Url { get; set; }
    public string InlineData { get; set; }
    public string Checksum { get; set; }
    public long Size { get; set; }
}