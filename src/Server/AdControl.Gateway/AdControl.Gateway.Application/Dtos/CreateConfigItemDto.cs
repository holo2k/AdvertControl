namespace AdControl.Gateway.Application.Dtos;

public class CreateConfigItemDto
{
    public int DurationSeconds;
    public int Order;
    public string? Id { get; set; }
    public string? Type { get; set; }
    public string? UrlOrData { get; set; }
    public string Url { get; set; }
    public string InlineData { get; set; }
    public string Checksum { get; set; }
    public long Size { get; set; }
}