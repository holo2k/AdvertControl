using System.Xml.Linq;
using AdControl.Gateway.Application.Dtos;
using AdControl.Protos;
using Microsoft.AspNetCore.Http.HttpResults;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace AdControl.Gateway.Mapper
{
    public static class ScreenMapper
    {
        public static ScreenDto? MapToScreenDto(this Screen screen)
        {
            if (screen.Id.Length <=0)
            {
                return null;
            }

            var lastHb = FromUnixMs(screen.LastHeartbeatAt);
            var pairedAt = FromUnixMs(screen.PairedAt);
            var diff = DateTime.UtcNow - lastHb;

            var screenDto = new ScreenDto
            {
                Id = Guid.Parse(screen.Id),
                CreatedAt = FromUnixMs(screen.CreatedAt),
                UpdatedAt = FromUnixMs(screen.UpdatedAt),
                UserId = Guid.Parse(screen.UserId),
                Name = screen.Name,
                Resolution = screen.Resolution,
                Location = screen.Location,
                LastHeartbeatAt = lastHb,
                PairedAt = pairedAt
            };

            if (pairedAt.Year < 2025)
                screenDto.Status = "NOT_PAIRED";
            else if (pairedAt.Year >= 2025 && lastHb.Year < 2025)
                screenDto.Status = "PAIRED";
            else if (diff > TimeSpan.FromMinutes(3))
                screenDto.Status = "ERROR";
            else
                screenDto.Status = "SUCCESS";

            return screenDto;
        }

        private static DateTime FromUnixMs(long value) =>
            DateTimeOffset.FromUnixTimeMilliseconds(value).UtcDateTime;
    }
}
