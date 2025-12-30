using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Service
    {
        [Key]
        public int ServiceId { get; set; }

        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ServiceDescription { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(255)]
        public string? ServiceIconPath { get; set; }

        // Navigation Properties
        public virtual ServiceCategory Category { get; set; } = null!;
        public virtual ICollection<ProviderServiceEntity> ProviderServices { get; set; } = new List<ProviderServiceEntity>();
        public virtual ICollection<ProviderWorkingHours> WorkingHours { get; set; } = new List<ProviderWorkingHours>();
        public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
        public virtual ICollection<EmergencyRequest> EmergencyRequests { get; set; } = new List<EmergencyRequest>();
    }
}