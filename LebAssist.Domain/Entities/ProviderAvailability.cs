using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ProviderAvailability
    {
        [Key]
        public int AvailabilityId { get; set; }

        public int ClientId { get; set; }

        public bool IsAvailable { get; set; } = false;

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(9,6)")]
        public decimal? CurrentLatitude { get; set; }

        [Column(TypeName = "decimal(9,6)")]
        public decimal? CurrentLongitude { get; set; }

        // Navigation Property
        public virtual Client Provider { get; set; } = null!;
    }
}