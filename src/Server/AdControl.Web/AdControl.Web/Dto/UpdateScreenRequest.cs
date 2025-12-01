namespace AdControl.Web.Dto;

public class UpdateScreenRequest
{
    public Guid Id {get; set; }
    public string Name { get; set; }
    public string Resolution { get; set; }
    public string Location { get; set; }
}