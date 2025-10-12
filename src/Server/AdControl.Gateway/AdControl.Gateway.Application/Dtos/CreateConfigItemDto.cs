namespace AdControl.Gateway.Application.Dtos;

public class CreateConfigItemDto
{
    public int DurationSeconds;
    public int Order;

    public CreateConfigItemDto(int durationSeconds, int order, string? id, string? type, string? urlOrData)
    {
        DurationSeconds = durationSeconds;
        Order = order;
        Id = id;
        Type = type;
        UrlOrData = urlOrData;
    }

    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? UrlOrData { get; set; }
}