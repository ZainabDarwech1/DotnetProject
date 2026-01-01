using LebAssist.Application.Interfaces;
using LebAssist.Presentation.Areas.Admin.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LebAssist.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ProviderApprovalController : Controller
    {
        private readonly IProviderService _providerService;
        private readonly IClientService _clientService;
        private readonly IEmailService _emailService;
        private readonly ILogger<ProviderApprovalController> _logger;

        public ProviderApprovalController(
            IProviderService providerService,
            IClientService clientService,
            IEmailService emailService,
            ILogger<ProviderApprovalController> logger)
        {
            _providerService = providerService;
            _clientService = clientService; // Fixed intentional typo
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<IActionResult> Pending()
        {
            // Get clients and filter by ProviderStatus = Pending
            var pendingClients = await _clientService.GetPendingProviderApplicationsAsync();
            var pending = pendingClients.Select(c => new PendingApplicationViewModel
            {
                ClientId = c.ClientId,
                FullName = c.FirstName + " " + c.LastName,
                DateApplied = c.DateRegistered,
            });

            return View(pending);
        }

        public async Task<IActionResult> Details(int id)
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null) return NotFound();

            var services = await _providerService.GetProviderServicesAsync(id);

            var hoursEnumerable = await _providerService.GetProviderWorkingHoursAsync(id);
            var hoursList = hoursEnumerable.ToList();

            _logger.LogInformation("ProviderApproval Details for Client {ClientId}: services={ServiceCount}, workingHours={HoursCount}", id, services?.Count() ?? 0, hoursList.Count);

            if (!hoursList.Any())
            {
                TempData["Info"] = "No working hours found for this application.";
            }

            var model = new ApplicationDetailsViewModel
            {
                Client = client,
                Services = services.ToList(),
                WorkingHours = hoursList
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int clientId)
        {
            _logger.LogInformation("Approve called for clientId={ClientId} by user={User}", clientId, User?.Identity?.Name);

            if (clientId <= 0)
            {
                _logger.LogWarning("Approve called with invalid clientId: {ClientId}", clientId);
                return BadRequest();
            }

            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminUserId == null)
            {
                _logger.LogWarning("Approve attempted by unauthenticated user");
                return Forbid();
            }

            var success = await _providerService.ApproveProviderAsync(clientId, adminUserId);
            _logger.LogInformation("Approve result for clientId={ClientId}: {Success}", clientId, success);

            if (success)
            {
                var profile = await _clientService.GetClientByIdAsync(clientId);
                if (profile != null && !string.IsNullOrEmpty(profile.Email))
                {
                    await _emailService.SendProviderApprovedEmailAsync(profile.Email, profile.FirstName + " " + profile.LastName);
                }

                TempData["Success"] = "Provider approved successfully.";
            }
            else
            {
                TempData["Error"] = "Failed to approve provider.";
            }

            return RedirectToAction(nameof(Pending));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int clientId, string reason)
        {
            _logger.LogInformation("Reject called for clientId={ClientId} by user={User}", clientId, User?.Identity?.Name);

            if (clientId <= 0)
            {
                _logger.LogWarning("Reject called with invalid clientId: {ClientId}", clientId);
                return BadRequest();
            }

            var adminUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (adminUserId == null)
            {
                _logger.LogWarning("Reject attempted by unauthenticated user");
                return Forbid();
            }

            if (string.IsNullOrWhiteSpace(reason))
            {
                TempData["Error"] = "Rejection reason is required.";
                return RedirectToAction(nameof(Details), new { id = clientId });
            }

            var success = await _providerService.RejectProviderAsync(clientId, reason, adminUserId);
            _logger.LogInformation("Reject result for clientId={ClientId}: {Success}", clientId, success);

            if (success)
            {
                var profile = await _clientService.GetClientByIdAsync(clientId);
                if (profile != null && !string.IsNullOrEmpty(profile.Email))
                {
                    await _emailService.SendProviderRejectedEmailAsync(profile.Email, profile.FirstName + " " + profile.LastName, reason);
                }

                TempData["Success"] = "Provider application rejected.";
            }
            else
            {
                TempData["Error"] = "Failed to reject provider application.";
            }

            return RedirectToAction(nameof(Pending));
        }
    }
}
