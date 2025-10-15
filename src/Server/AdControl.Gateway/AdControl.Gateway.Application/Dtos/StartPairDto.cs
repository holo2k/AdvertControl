namespace AdControl.Gateway.Application.Dtos;

public class StartPairDto
{
    public string TempDisplayId { get; set; } = default!;
    public string Code { get; set; } = default!;
    public int TtlMinutes { get; set; } = 10;
    public string? Info { get; set; }
}