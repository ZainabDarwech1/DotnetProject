using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ProviderPortfolio
    {
        [Key]
        public int PortfolioPhotoId { get; set; }

        public int ClientId { get; set; }

        [Required]
        [MaxLength(255)]
        public string PhotoPath { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Caption { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public int DisplayOrder { get; set; } = 0;

        // Navigation Property
        public virtual Client Provider { get; set; } = null!;
    }
}