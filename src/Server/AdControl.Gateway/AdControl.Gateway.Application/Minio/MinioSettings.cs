namespace AdControl.Gateway.Application.Minio;

public class MinioSettings
{
    public string Endpoint { get; set; } = "";
    public string AccessKey { get; set; } = "";
    public string SecretKey { get; set; } = "";
    public bool Secure { get; set; } = false;
    public int PresignExpirySeconds { get; set; } = 300;
}