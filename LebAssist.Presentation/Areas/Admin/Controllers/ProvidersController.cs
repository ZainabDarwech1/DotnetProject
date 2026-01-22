using LebAssist.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LebAssist.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProvidersController : Controller
    {
        private readonly IProviderService _providerService;
        private readonly IClientService _clientService;
        private readonly ILogger<ProvidersController> _logger;

        public ProvidersController(
            IProviderService providerService,
            IClientService clientService,
            ILogger<ProvidersController> logger)
        {
            _providerService = providerService;
            _clientService = clientService;
            _logger = logger;
        }

        // GET: /Admin/Providers/Index
        public async Task<IActionResult> Index()
        {
            var providers = await _providerService.GetAllActiveProvidersAsync();
            return View(providers);
        }

        // GET: /Admin/Providers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var provider = await _clientService.GetClientByIdAsync(id);
            if (provider == null || !provider.IsProvider)
            {
                TempData["ErrorMessage"] = "Provider not found.";
                return RedirectToAction(nameof(Index));
            }

            var services = await _providerService.GetProviderServicesAsync(id);
            var portfolio = await _providerService.GetProviderPortfolioAsync(id);

            ViewBag.Services = services;
            ViewBag.Portfolio = portfolio;

            return View(provider);
        }

        // POST: /Admin/Providers/Deactivate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int clientId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
            {
                return Json(new { success = false, message = "Deactivation reason is required." });
            }

            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(adminUserId))
            {
                return Json(new { success = false, message = "Unauthorized." });
            }

            var result = await _providerService.DeactivateProviderAsync(clientId, adminUserId, reason);

            if (result)
            {
                TempData["SuccessMessage"] = "Provider deactivated successfully.";
                return Json(new { success = true });
            }
            else
            {
                return Json(new { success = false, message = "Failed to deactivate provider. They may have active bookings." });
            }
        }
    }
}
