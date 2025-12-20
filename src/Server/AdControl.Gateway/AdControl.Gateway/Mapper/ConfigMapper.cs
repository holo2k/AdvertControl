using AdControl.Gateway.Application.Dtos;
using AdControl.Protos;

namespace AdControl.Gateway.Mapper
{
    public static class ConfigMapper
    {
        public static ConfigDto? MapToConfigDto(this Config config)
        {   
            if (config.Id.Length <= 0)
            {
                return null;
            }

            var configDto = new ConfigDto
            {
                Id = Guid.Parse(config.Id),
                CreatedAt = FromUnixMs(config.CreatedAt),
                UpdatedAt = FromUnixMs(config.UpdatedAt),
                UserId = Guid.Parse(config.UserId),
                Name = config.Name,
                ScreensCount = config.ScreensCount,
                IsStatic = config.IsStatic,
                Version = config.Version
            };

            foreach (var it in config.Items)
            {
                configDto.Items.Add(new ConfigItemDto
                {
                    Id = Guid.Parse(it.Id),
                    Type = it.Type.ToString().ToUpperInvariant(),
                    Url = it.Url,
                    InlineData = it.InlineData,
                    Checksum = it.Checksum ?? string.Empty,
                    Size = it.Size,
                    DurationSeconds = it.DurationSeconds,
                    Order = it.Order
                });
            }

            return configDto;
        }

        private static DateTime FromUnixMs(long value) =>
            DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime;
    }

}
