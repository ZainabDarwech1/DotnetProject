using LebAssist.Application.Interfaces;
using LebAssist.Presentation.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LebAssist.Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ICategoryService _categoryService;
        private readonly IDashboardService _dashboardService;

        public HomeController(
            ILogger<HomeController> logger,
            ICategoryService categoryService,
            IDashboardService dashboardService)
        {
            _logger = logger;
            _categoryService = categoryService;
            _dashboardService = dashboardService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Get featured categories for the landing page
                var categories = await _categoryService.GetActiveCategoriesAsync();
                ViewBag.Categories = categories.Take(8).ToList();

                // Get some statistics for social proof
                var stats = await _dashboardService.GetDashboardStatsAsync();
                ViewBag.Stats = stats;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page");
                return View();
            }
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}