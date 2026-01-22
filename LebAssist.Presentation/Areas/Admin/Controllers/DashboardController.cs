using LebAssist.Application.Interfaces;
using LebAssist.Presentation.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LebAssist.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IDashboardService dashboardService,
            ILogger<DashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var stats = await _dashboardService.GetDashboardStatsAsync();
                var bookingTrends = await _dashboardService.GetBookingTrendsAsync(6);
                var categoryStats = await _dashboardService.GetCategoryStatsAsync();
                var topServices = await _dashboardService.GetTopServicesAsync(10);
                var topProviders = await _dashboardService.GetTopProvidersAsync(10);
                var statusDistribution = await _dashboardService.GetBookingStatusDistributionAsync();
                var userGrowth = await _dashboardService.GetUserGrowthAsync(12);
                var emergencyStats = await _dashboardService.GetEmergencyStatsAsync(30);

                var viewModel = new DashboardViewModel
                {
                    Stats = stats,
                    BookingTrends = bookingTrends,
                    CategoryStats = categoryStats,
                    TopServices = topServices,
                    TopProviders = topProviders,
                    BookingStatusDistribution = statusDistribution,
                    UserGrowth = userGrowth,
                    EmergencyStats = emergencyStats
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading dashboard");
                TempData["ErrorMessage"] = "An error occurred while loading the dashboard.";
                return View(new DashboardViewModel());
            }
        }
    }
}
