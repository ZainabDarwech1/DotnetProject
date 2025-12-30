using LebAssist.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LebAssist.Presentation.Controllers
{
    [Authorize(Roles = "Provider")]
    public class ProviderBookingsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IClientService _clientService;

        public ProviderBookingsController(
            IBookingService bookingService,
            IClientService clientService)
        {
            _bookingService = bookingService;
            _clientService = clientService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            var bookings = await _bookingService.GetProviderBookingsAsync(profile.ClientId);
            return View(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> Accept(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            await _bookingService.AcceptBookingAsync(bookingId, profile.ClientId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(int bookingId, string? reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            await _bookingService.RejectBookingAsync(bookingId, profile.ClientId, reason);
            return RedirectToAction(nameof(Index));
        }
    }
}