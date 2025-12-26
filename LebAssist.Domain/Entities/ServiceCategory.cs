using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class ServiceCategory
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [MaxLength(255)]
        public string? IconPath { get; set; }

        public bool IsActive { get; set; } = true;

        public int DisplayOrder { get; set; } = 0;

        // Navigation Property
        public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    }
}