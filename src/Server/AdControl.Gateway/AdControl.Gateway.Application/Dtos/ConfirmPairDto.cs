namespace AdControl.Gateway.Application.Dtos;

public class ConfirmPairDto
{
    public int AssignedTtlMinutes;

    public ConfirmPairDto(string code, string? name, string? location, string? resolution, int assignedTtlMinutes = 60)
    {
        AssignedTtlMinutes = assignedTtlMinutes;
        Code = code;
        Name = name;
        Location = location;
        Resolution = resolution;
    }

    public string Code { get; set; }
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? Resolution { get; set; }
}