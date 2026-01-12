using System.Text.Json.Serialization;

namespace AdControl.Gateway.Application.Dtos;

/// <summary>
///     DTO для старта привязки экрана.
/// </summary>
public class StartPairDto
{
    /// <summary>
    ///     Временный идентификатор экрана.
    /// </summary>
    [JsonPropertyName("tempDisplayId")]
    public string TempDisplayId { get; set; } = default!;

    /// <summary>
    ///     Код привязки.
    /// </summary>
    [JsonPropertyName("code")]
    public string Code { get; set; } = default!;

    /// <summary>
    ///     Время жизни кода в минутах.
    /// </summary>
    [JsonPropertyName("ttlMinutes")]
    public int TtlMinutes { get; set; } = 10;

    /// <summary>
    ///     Дополнительная информация о экране.
    /// </summary>
    [JsonPropertyName("info")]
    public string? Info { get; set; }
}