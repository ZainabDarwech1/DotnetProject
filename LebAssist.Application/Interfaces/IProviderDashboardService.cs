using LebAssist.Application.DTOs;

namespace LebAssist.Application.Interfaces
{
    public interface IProviderDashboardService
    {
        Task<ProviderDashboardStatsDto> GetProviderDashboardStatsAsync(int providerId);
        Task<IEnumerable<ProviderBookingTrendDto>> GetProviderBookingTrendsAsync(int providerId, int months = 6);
        Task<IEnumerable<ProviderRevenueByMonthDto>> GetProviderRevenueByMonthAsync(int providerId, int months = 12);
        Task<IEnumerable<ProviderServiceRevenueDto>> GetProviderServiceRevenueAsync(int providerId);
        Task<IEnumerable<ProviderBookingStatusDto>> GetProviderBookingStatusDistributionAsync(int providerId);
        Task<IEnumerable<ProviderRecentBookingDto>> GetProviderRecentBookingsAsync(int providerId, int count = 5);
        
        // Calendar methods
        Task<ProviderCalendarDto> GetProviderCalendarAsync(int providerId, int year, int month);
        Task<List<AppointmentDto>> GetProviderAppointmentsForDayAsync(int providerId, DateTime date);
    }
}
