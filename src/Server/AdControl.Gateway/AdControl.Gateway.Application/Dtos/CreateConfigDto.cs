namespace AdControl.Gateway.Application.Dtos;

public class CreateConfigDto
{
    public string? UserId { get; set; }
    public List<CreateConfigItemDto>? Items { get; set; }
}