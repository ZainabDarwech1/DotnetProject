namespace LebAssist.Application.DTOs
{
    public class ProviderServiceDto
    {
        public int ProviderServiceId { get; set; }
        public int ServiceId { get; set; }
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
    }
}