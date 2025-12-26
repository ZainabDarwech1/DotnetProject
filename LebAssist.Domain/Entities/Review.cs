using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        public int BookingId { get; set; }

        public int ClientId { get; set; }

        public int ProviderId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        public bool IsVisible { get; set; } = true;

        // Navigation Properties
        public virtual Booking Booking { get; set; } = null!;
        public virtual Client Client { get; set; } = null!;
        public virtual Client Provider { get; set; } = null!;
    }
}