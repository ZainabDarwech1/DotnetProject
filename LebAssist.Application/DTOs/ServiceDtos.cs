using System.ComponentModel.DataAnnotations;

namespace LebAssist.Application.DTOs
{
    public class ServiceDto
    {
        public int ServiceId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string? ServiceDescription { get; set; }
        public bool IsActive { get; set; }
        public string? ServiceIconPath { get; set; }
    }

    public class CreateServiceDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ServiceDescription { get; set; }

        public byte[]? ServiceIconData { get; set; }
        public string? ServiceIconFileName { get; set; }
    }

    public class UpdateServiceDto
    {
        [Required]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? ServiceDescription { get; set; }

        public byte[]? ServiceIconData { get; set; }
        public string? ServiceIconFileName { get; set; }
    }
}
