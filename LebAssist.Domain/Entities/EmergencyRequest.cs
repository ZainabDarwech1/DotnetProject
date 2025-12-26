using Domain.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class EmergencyRequest
    {
        [Key]
        public int EmergencyRequestId { get; set; }

        public int ClientId { get; set; }

        public int ServiceId { get; set; }

        public int? ProviderId { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal Longitude { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Details { get; set; } = string.Empty;

        public DateTime RequestDateTime { get; set; } = DateTime.UtcNow;

        public DateTime? AcceptedDateTime { get; set; }

        public EmergencyStatus Status { get; set; } = EmergencyStatus.Pending;

        public DateTime? CompletedDate { get; set; }

        // Navigation Properties
        public virtual Client Client { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
        public virtual Client? Provider { get; set; }
    }
}