using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities
{
    public class ProviderService
    {
        [Key]
        public int ProviderServiceId { get; set; }

        public int ClientId { get; set; }

        public int ServiceId { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime DateAdded { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "decimal(10,2)")]
        public decimal? PricePerHour { get; set; }

        // Navigation Properties
        public virtual Client Provider { get; set; } = null!;
        public virtual Service Service { get; set; } = null!;
    }
}