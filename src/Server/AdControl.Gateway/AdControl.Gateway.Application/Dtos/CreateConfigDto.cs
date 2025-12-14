namespace AdControl.Gateway.Application.Dtos;

/// <summary>
///     DTO для создания конфигурации.
/// </summary>
public class CreateConfigDto
{
    /// <summary>
    /// Название конфига
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Количество экранов, на которые будет выводиться конфиг
    /// </summary>
    public int ScreensCount { get; set; }

    /// <summary>
    ///     Список элементов конфигурации.
    /// </summary>
    public List<CreateConfigItemDto>? Items { get; set; } 
}