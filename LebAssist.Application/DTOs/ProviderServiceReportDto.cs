namespace LebAssist.Application.DTOs
{
    public class ProviderServiceReportDto
    {
        public string ProviderName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<ServiceRevenueDto> Services { get; set; } = new();
        public int TotalServicesProvided { get; set; }
        public decimal TotalRevenue { get; set; }
        public DateTime ReportGeneratedDate { get; set; } = DateTime.UtcNow;
    }

    public class ServiceRevenueDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public int TimesProvided { get; set; }
        public decimal PricePerService { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
