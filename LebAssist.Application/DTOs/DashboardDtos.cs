namespace LebAssist.Application.DTOs
{
    public class DashboardStatsDto
    {
        public int TotalUsers { get; set; }
        public int TotalProviders { get; set; }
        public int TotalClients { get; set; }
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int TotalCategories { get; set; }
        public int TotalServices { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
        public int TotalEmergencies { get; set; }
        public int PendingEmergencies { get; set; }
        public int NewUsersThisMonth { get; set; }
        public int BookingsThisMonth { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
    }

    public class BookingTrendDto
    {
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
    }

    public class CategoryStatsDto
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public int ServiceCount { get; set; }
        public int BookingCount { get; set; }
        public int ProviderCount { get; set; }
    }

    public class ServicePopularityDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public int BookingCount { get; set; }
        public int ProviderCount { get; set; }
        public double AverageRating { get; set; }
    }

    public class ProviderPerformanceDto
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string? ProfilePhotoPath { get; set; }
        public int CompletedBookings { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public decimal TotalRevenue { get; set; }
    }

    public class RevenueByMonthDto
    {
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
    }

    public class BookingStatusDistributionDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class UserGrowthDto
    {
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public int NewUsers { get; set; }
        public int NewProviders { get; set; }
        public int TotalUsers { get; set; }
    }

    public class EmergencyStatsDto
    {
        public string Date { get; set; } = string.Empty;
        public int TotalEmergencies { get; set; }
        public int AcceptedEmergencies { get; set; }
        public int PendingEmergencies { get; set; }
    }
}
