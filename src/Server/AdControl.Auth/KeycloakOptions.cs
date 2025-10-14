namespace AdControl.Auth;

public class KeycloakOptions
{
    public string? AdminUser { get; set; }
    public string? AdminPassword { get; set; }
    public string? BaseUrl { get; set; }
    public string? AdminClientId { get; set; }
    public string? AdminClientSecret { get; set; }
}