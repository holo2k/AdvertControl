namespace AdControl.Gateway.Application.Dtos;

public class StartPairDto
{
    public int TtlMinutes;

    public StartPairDto(string tempDisplayId, string code, string? info, int ttlMinutes = 10)
    {
        TtlMinutes = ttlMinutes;
        TempDisplayId = tempDisplayId;
        Code = code;
        Info = info;
    }

    public string TempDisplayId { get; set; }
    public string Code { get; set; }
    public string? Info { get; set; }
}