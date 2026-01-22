using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
        Task<IEnumerable<BookingTrendDto>> GetBookingTrendsAsync(int months = 6);
        Task<IEnumerable<CategoryStatsDto>> GetCategoryStatsAsync();
        Task<IEnumerable<ServicePopularityDto>> GetTopServicesAsync(int count = 10);
        Task<IEnumerable<ProviderPerformanceDto>> GetTopProvidersAsync(int count = 10);
        Task<IEnumerable<RevenueByMonthDto>> GetRevenueByMonthAsync(int months = 12);
        Task<IEnumerable<BookingStatusDistributionDto>> GetBookingStatusDistributionAsync();
        Task<IEnumerable<UserGrowthDto>> GetUserGrowthAsync(int months = 12);
        Task<IEnumerable<EmergencyStatsDto>> GetEmergencyStatsAsync(int days = 30);
    }
}
