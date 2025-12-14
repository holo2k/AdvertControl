namespace AdControl.Gateway.Application.Dtos;

/// <summary>
///     DTO для одного элемента конфигурации.
/// </summary>
public class CreateConfigItemDto
{
    /// <summary>
    ///     Продолжительность показа элемента в секундах.
    /// </summary>
    public int DurationSeconds { get; set; }

    /// <summary>
    ///     Порядок элемента в конфигурации.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    ///     Идентификатор элемента.
    /// </summary>
    public string? Id { get; set; }

    /// <summary>
    ///     Тип элемента.
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    ///     URL элемента.
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    ///     Встроенные данные (json-таблица) элемента.
    /// </summary>
    public string? InlineData { get; set; }

    /// <summary>
    ///     Контрольная сумма элемента.
    /// </summary>
    public string Checksum { get; set; }

    /// <summary>
    ///     Размер элемента в байтах.
    /// </summary>
    public long Size { get; set; }
}