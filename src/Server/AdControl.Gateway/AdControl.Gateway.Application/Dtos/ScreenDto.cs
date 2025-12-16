namespace AdControl.Gateway.Application.Dtos
{
    public class ScreenDto
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string Name { get; set; } = "";
        public string Resolution { get; set; } = "";
        public string Location { get; set; } = "";
        public DateTime? LastHeartbeatAt { get; set; }
        public DateTime? PairedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
