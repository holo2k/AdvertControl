namespace AdControl.Gateway.Application.Dtos;

public class AssignDto
{
    public AssignDto(string screenId, bool isActive)
    {
        ScreenId = screenId;
        IsActive = isActive;
    }

    public string ScreenId { get; set; }
    public bool IsActive { get; set; }
}