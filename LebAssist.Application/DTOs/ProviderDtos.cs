namespace LebAssist.Application.DTOs
{
    public class ProviderServiceDto
    {
        public int ProviderServiceId { get; set; }
        public int ServiceId { get; set; }
        public int CategoryId { get; set; } // Added category id
        public string ServiceName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public decimal PricePerHour { get; set; }
        public DateTime DateAdded { get; set; }
    }

    public class AddProviderServiceDto
    {
        public int ServiceId { get; set; }
        public decimal PricePerHour { get; set; }
    }

    public class PortfolioPhotoDto
    {
        public int PortfolioPhotoId { get; set; }
        public string PhotoPath { get; set; } = string.Empty;
        public string? Caption { get; set; }
        public DateTime UploadDate { get; set; }
        public int DisplayOrder { get; set; }
    }

    public class ProviderApplicationDto
    {
        public string Bio { get; set; } = string.Empty;
        public int YearsOfExperience { get; set; }
        public List<int> SelectedServiceIds { get; set; } = new();
        public List<ProviderWorkingHoursDto> ServiceWorkingHours { get; set; } = new();
    }

    public class ProviderWorkingHoursDto
    {
        public int ServiceId { get; set; }
        public List<DayScheduleDto> DaySchedules { get; set; } = new();
    }

    public class DayScheduleDto
    {
        public int DayOfWeek { get; set; } // 0 = Sunday, 1 = Monday, ..., 6 = Saturday
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
    }

    public class ProviderOfferingDto
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string? ProviderPhotoPath { get; set; }
        public int ProviderServiceId { get; set; }
        public decimal PricePerHour { get; set; }
    }

    public class ProviderListDto
    {
        public int ClientId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName => $"{FirstName} {LastName}";
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? ProfilePhotoPath { get; set; }
        public int? YearsOfExperience { get; set; }
        public DateTime DateRegistered { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int TotalServices { get; set; }
        public int CompletedBookings { get; set; }
        public decimal? AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public bool IsActive { get; set; }
    }
}