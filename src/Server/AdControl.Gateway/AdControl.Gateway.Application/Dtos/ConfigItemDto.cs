

namespace AdControl.Gateway.Application.Dtos
{
    public class ConfigItemDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = ""; // "IMAGE", "VIDEO", "TABLE", "INLINE_JSON"
        public string? Url { get; set; } // для файлов в MinIO/S3
        public string? InlineData { get; set; } // для текст/JSON таблиц
        public string? Checksum { get; set; }
        public long Size { get; set; }
        public int DurationSeconds { get; set; }
        public int Order { get; set; }
    }
}
