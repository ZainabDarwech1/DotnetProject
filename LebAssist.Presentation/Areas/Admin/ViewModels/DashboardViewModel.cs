using LebAssist.Application.DTOs;

namespace LebAssist.Presentation.Areas.Admin.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardStatsDto Stats { get; set; } = new();
        public IEnumerable<BookingTrendDto> BookingTrends { get; set; } = new List<BookingTrendDto>();
        public IEnumerable<CategoryStatsDto> CategoryStats { get; set; } = new List<CategoryStatsDto>();
        public IEnumerable<ServicePopularityDto> TopServices { get; set; } = new List<ServicePopularityDto>();
        public IEnumerable<ProviderPerformanceDto> TopProviders { get; set; } = new List<ProviderPerformanceDto>();
        public IEnumerable<BookingStatusDistributionDto> BookingStatusDistribution { get; set; } = new List<BookingStatusDistributionDto>();
        public IEnumerable<UserGrowthDto> UserGrowth { get; set; } = new List<UserGrowthDto>();
        public IEnumerable<EmergencyStatsDto> EmergencyStats { get; set; } = new List<EmergencyStatsDto>();
    }
}
