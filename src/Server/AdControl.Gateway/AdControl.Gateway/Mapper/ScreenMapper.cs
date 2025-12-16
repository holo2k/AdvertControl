using System.Xml.Linq;
using AdControl.Gateway.Application.Dtos;
using AdControl.Protos;
using Microsoft.AspNetCore.Http.HttpResults;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace AdControl.Gateway.Mapper
{
    public static class ScreenMapper
    {
        public static ScreenDto MapToScreenDto(this Screen screen)
        {
            var screenDto = new ScreenDto
            {
                Id = Guid.Parse(screen.Id),
                CreatedAt = FromUnixMs(screen.CreatedAt),
                UpdatedAt = FromUnixMs(screen.UpdatedAt),
                UserId = Guid.Parse(screen.UserId),
                Name = screen.Name,
                Resolution = screen.Resolution,
                Location = screen.Location,
                LastHeartbeatAt = FromUnixMs(screen.LastHeartbeatAt),
                PairedAt = FromUnixMs(screen.PairedAt)
            };

            return screenDto;
        }

        private static DateTime FromUnixMs(long value) =>
            DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime;
    }
}
