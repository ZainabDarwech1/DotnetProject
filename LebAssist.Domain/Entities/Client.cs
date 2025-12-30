using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }

        [Required]
        public string AspNetUserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        [MaxLength(255)]
        public string? ProfilePhotoPath { get; set; }

        public bool IsProvider { get; set; } = false;

        public ProviderStatus? ProviderStatus { get; set; }

        [MaxLength(500)]
        public string? Bio { get; set; }

        public int? YearsOfExperience { get; set; }

        public DateTime DateRegistered { get; set; } = DateTime.UtcNow;
        public decimal? ProviderAverageRating { get; set; }  // e.g. 4.7
        public int TotalReviews { get; set; } = 0;


        // Navigation Properties
        public virtual ICollection<ProviderService> ProviderServices { get; set; } = new List<ProviderService>();
        public virtual ICollection<ProviderWorkingHours> WorkingHours { get; set; } = new List<ProviderWorkingHours>();
        public virtual ICollection<ProviderPortfolio> PortfolioPhotos { get; set; } = new List<ProviderPortfolio>();
        public virtual ICollection<Booking> ClientBookings { get; set; } = new List<Booking>();
        public virtual ICollection<Booking> ProviderBookings { get; set; } = new List<Booking>();
        public virtual ICollection<EmergencyRequest> ClientEmergencies { get; set; } = new List<EmergencyRequest>();
        public virtual ICollection<EmergencyRequest> ProviderEmergencies { get; set; } = new List<EmergencyRequest>();
        public virtual ICollection<Review> ReviewsWritten { get; set; } = new List<Review>();
        public virtual ICollection<Review> ReviewsReceived { get; set; } = new List<Review>();
        public virtual ICollection<Report> ReportsSubmitted { get; set; } = new List<Report>();
        public virtual ICollection<Report> ReportsReceived { get; set; } = new List<Report>();
        public virtual ProviderAvailability? Availability { get; set; }
    }
}