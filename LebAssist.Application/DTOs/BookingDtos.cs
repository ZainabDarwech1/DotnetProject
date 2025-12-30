using Domain.Enums;

namespace LebAssist.Application.DTOs
{
    // Your existing BookingDtos class
    public class BookingDtos
    {
        public int ProviderId { get; set; }
        public int ServiceId { get; set; }
        public DateTime BookingDateTime { get; set; }
        public string LocationAddress { get; set; } = string.Empty;
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string? Notes { get; set; }
    }

    // NEW DTO for booking details
    public class BookingDetailsDto
    {
        public int BookingId { get; set; }
        public int ClientId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string? ClientPhotoPath { get; set; }
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string? ProviderPhotoPath { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public DateTime RequestDate { get; set; }
        public DateTime ScheduledDateTime { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public BookingStatus Status { get; set; }
        public string StatusName => Status.ToString();
        public string? Notes { get; set; }
        public DateTime? CompletedDate { get; set; }
        public string? CancellationReason { get; set; }
        public bool HasReview { get; set; }
        public int? ReviewId { get; set; }
    }
}