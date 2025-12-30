using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Review
    {
        public int ReviewId { get; set; }

        // one review per booking
        public int BookingId { get; set; }

        // reviewer client
        public int ClientId { get; set; }

        // reviewed provider (also in Client table)
        public int ProviderId { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(1000)]
        public string? Comment { get; set; }

        public DateTime ReviewDate { get; set; } = DateTime.UtcNow;

        public bool IsVisible { get; set; } = true;

        // Optional features you mentioned:
        public bool IsAnonymous { get; set; } = false;

        // Useful for moderation workflows
        public bool AdminModerated { get; set; } = false;

        // Navigation
        public Booking Booking { get; set; } = null!;
        public Client Client { get; set; } = null!;
        public Client Provider { get; set; } = null!;
    }
}
