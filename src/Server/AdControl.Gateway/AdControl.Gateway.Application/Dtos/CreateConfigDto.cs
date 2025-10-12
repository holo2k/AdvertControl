namespace AdControl.Gateway.Application.Dtos;

public class CreateConfigDto
{
    public CreateConfigDto(string? userId, List<CreateConfigItemDto>? items)
    {
        UserId = userId;
        Items = items;
    }

    public string? UserId { get; set; }
    public List<CreateConfigItemDto>? Items { get; set; }
}