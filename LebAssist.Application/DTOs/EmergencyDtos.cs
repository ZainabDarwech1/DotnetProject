namespace LebAssist.Application.DTOs
{
    public class EmergencyDtos
    {
        public int ServiceId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string LocationAddress { get; set; } = string.Empty;  // Added default value
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public byte[]? PhotoData { get; set; }
        public string? PhotoFileName { get; set; }
    }
}