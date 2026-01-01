using LebAssist.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace LebAssist.Presentation.ViewModels.Provider
{
    public class ProviderProfileViewModel
    {
        public int ProviderId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string? ProfilePhotoPath { get; set; }
        public string? Bio { get; set; }
        public int? YearsOfExperience { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public bool IsAvailable { get; set; }

        public List<ProviderServiceDto> Services { get; set; } = new();
        public List<PortfolioPhotoDto> Portfolio { get; set; } = new();
        public ProviderReviewsSummaryDto ReviewsSummary { get; set; } = new();
    }

    // Add this to update the Dashboard ViewModel
    public class ProviderDashboardViewModel
    {
        public string ProviderName { get; set; } = string.Empty;
        public int TotalServices { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public List<ReviewDto> RecentReviews { get; set; } = new();
    }

    // Your existing ViewModels below...
    public class ProviderServiceViewModel
    {
        public int ProviderServiceId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public decimal PricePerHour { get; set; }
        public DateTime DateAdded { get; set; }
    }

    public class ProviderApplicationViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MinLength(50, ErrorMessage = "Please provide at least 50 characters.")]
        [MaxLength(500, ErrorMessage = "Bio cannot exceed 500 characters.")]
        public string Bio { get; set; } = string.Empty;

        [Range(0, 50, ErrorMessage = "Years of experience must be between 0 and 50.")]
        public int YearsOfExperience { get; set; }
        public List<int> SelectedServiceIds { get; set; } = new();
        public List<ServiceCategoryGroup> ServiceCategories { get; set; } = new();

        // Profile photo (optional)
        public IFormFile? ProfilePhoto { get; set; }

        // Portfolio photos (optional, multiple)
        public List<IFormFile>? PortfolioPhotos { get; set; } = new();
    }

    public class ServiceCategoryGroup
    {
        public string CategoryName { get; set; } = string.Empty;
        public List<ServiceSelectItem> Services { get; set; } = new();
    }

    public class ServiceSelectItem
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
    }

    public class AddProviderServiceViewModel
    {
        public int ServiceId { get; set; }
        public decimal PricePerHour { get; set; }
        public List<ServiceCategoryGroup> ServiceCategories { get; set; } = new();
        public List<ProviderWorkingHoursDto> ServiceWorkingHours { get; set; } = new();
    }

    public class EditProviderServiceViewModel
    {
        public int ServiceId { get; set; }
        public int ProviderServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal PricePerHour { get; set; }
        public List<ProviderWorkingHoursDto> ServiceWorkingHours { get; set; } = new();
    }

    public class PortfolioViewModel
    {
        public List<PortfolioPhotoViewModel> Photos { get; set; } = new();
    }

    public class PortfolioPhotoViewModel
    {
        public int PortfolioPhotoId { get; set; }
        public string PhotoPath { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public DateTime UploadDate { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class AddPortfolioPhotoViewModel
    {
        public IFormFile Photo { get; set; } = null!;
        public string? Caption { get; set; }
    }
}