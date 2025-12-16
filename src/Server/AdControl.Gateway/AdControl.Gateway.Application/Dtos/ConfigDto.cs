namespace AdControl.Gateway.Application.Dtos
{
    public class ConfigDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid? UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public long Version { get; set; }
        public int ScreensCount { get; set; }
        public List<ConfigItemDto> Items { get; set; } = new();
    }
}
