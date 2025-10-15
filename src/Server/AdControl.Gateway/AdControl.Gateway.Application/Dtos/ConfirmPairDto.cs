namespace AdControl.Gateway.Application.Dtos;

public class ConfirmPairDto
{
    public string Code { get; set; } = null!;
    public string? Name { get; set; }
    public string? Resolution { get; set; }
    public string? Location { get; set; }
    public int AssignedTtlMinutes { get; set; }
}