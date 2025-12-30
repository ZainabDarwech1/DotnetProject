using System.ComponentModel.DataAnnotations;

namespace LebAssist.Application.DTOs
{
    public class CategoryDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconPath { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public int ServiceCount { get; set; }
    }

    public class CreateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public byte[]? IconData { get; set; }
        public string? IconFileName { get; set; }

        public int DisplayOrder { get; set; } = 0;
    }

    public class UpdateCategoryDto
    {
        [Required]
        [MaxLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public byte[]? IconData { get; set; }
        public string? IconFileName { get; set; }

        public int DisplayOrder { get; set; }
    }

    public class CategoryWithServicesDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconPath { get; set; }
        public bool IsActive { get; set; }
        public List<ServiceDto> Services { get; set; } = new();
    }
}
