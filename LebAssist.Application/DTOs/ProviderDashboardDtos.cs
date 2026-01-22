namespace LebAssist.Application.DTOs
{
    public class ProviderDashboardStatsDto
    {
        public int TotalBookings { get; set; }
        public int PendingBookings { get; set; }
        public int AcceptedBookings { get; set; }
        public int CompletedBookings { get; set; }
        public int CancelledBookings { get; set; }
        public int TotalServices { get; set; }
        public int ActiveServices { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public decimal RevenueLastMonth { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int NewReviewsThisMonth { get; set; }
        public int BookingsThisMonth { get; set; }
        public int BookingsLastMonth { get; set; }
        public decimal AverageRevenuePerBooking { get; set; }
    }

    public class ProviderBookingTrendDto
    {
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public int TotalBookings { get; set; }
        public int CompletedBookings { get; set; }
        public decimal Revenue { get; set; }
    }

    public class ProviderRevenueByMonthDto
    {
        public string Month { get; set; } = string.Empty;
        public int Year { get; set; }
        public decimal Revenue { get; set; }
        public int BookingCount { get; set; }
        public decimal AveragePerBooking { get; set; }
    }

    public class ProviderServiceRevenueDto
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal PricePerHour { get; set; }
        public int BookingCount { get; set; }
        public int CompletedBookings { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageRating { get; set; }
    }

    public class ProviderBookingStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    public class ProviderRecentBookingDto
    {
        public int BookingId { get; set; }
        public string ClientName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime ScheduledDateTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal EstimatedRevenue { get; set; }
    }
}
