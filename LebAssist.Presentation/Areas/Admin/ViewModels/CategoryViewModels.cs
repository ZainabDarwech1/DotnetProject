using System.ComponentModel.DataAnnotations;

namespace LebAssist.Presentation.Areas.Admin.ViewModels
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconPath { get; set; }
        public bool IsActive { get; set; }
        public int DisplayOrder { get; set; }
        public int ServiceCount { get; set; }
    }

    public class CreateCategoryViewModel
    {
        [Required]
        [MaxLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Icon")]
        public IFormFile? Icon { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; } = 0;
    }

    public class EditCategoryViewModel
    {
        public int CategoryId { get; set; }

        [Required]
        [MaxLength(100)]
        [Display(Name = "Category Name")]
        public string CategoryName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Display(Name = "Icon")]
        public IFormFile? Icon { get; set; }

        public string? CurrentIconPath { get; set; }

        [Display(Name = "Display Order")]
        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; }
    }
}
