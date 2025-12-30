using System.ComponentModel.DataAnnotations;

namespace LebAssist.Presentation.Areas.Admin.ViewModels
{
    public class ServiceViewModel
    {
        public int ServiceId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public string? ServiceDescription { get; set; }
        public bool IsActive { get; set; }
        public string? ServiceIconPath { get; set; }
    }

    public class CreateServiceViewModel
    {
        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Service Name")]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? ServiceDescription { get; set; }

        [Display(Name = "Icon")]
        public IFormFile? ServiceIcon { get; set; }

        public List<CategorySelectItem> Categories { get; set; } = new();
    }

    public class EditServiceViewModel
    {
        public int ServiceId { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Service Name")]
        public string ServiceName { get; set; } = string.Empty;

        [MaxLength(500)]
        [Display(Name = "Description")]
        public string? ServiceDescription { get; set; }

        [Display(Name = "Icon")]
        public IFormFile? ServiceIcon { get; set; }

        public string? CurrentIconPath { get; set; }
        public bool IsActive { get; set; }

        public List<CategorySelectItem> Categories { get; set; } = new();
    }

    public class CategorySelectItem
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
    }
}
