using Domain.Enums;
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
            _clientService = clientService; // fixed to _clientService
        }

        // Index now accepts optional status filter by name (e.g., Pending, Accepted)
        public async Task<IActionResult> Index(string? status)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            var bookings = await _bookingService.GetProviderBookingsAsync(profile.ClientId);

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<BookingStatus>(status, true, out var parsed))
                {
                    bookings = bookings.Where(b => b.Status == parsed);
                }
            }

            ViewBag.ActiveStatus = status ?? "All";
            return View(bookings);
        }

        // POST: Accept booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            await _bookingService.AcceptBookingAsync(bookingId, profile.ClientId);
            return RedirectToAction(nameof(Index), new { status = "Accepted" });
        }

        // POST: Reject booking
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int bookingId, string? reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            await _bookingService.RejectBookingAsync(bookingId, profile.ClientId, reason);
            return RedirectToAction(nameof(Index));
        }

        // POST: Start service (provider marks booking in progress)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Start(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            await _bookingService.StartBookingAsync(bookingId, profile.ClientId);
            return RedirectToAction(nameof(Index), new { status = "InProgress" });
        }

        // POST: Complete service
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Complete(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            await _bookingService.CompleteBookingAsync(bookingId, profile.ClientId);
            return RedirectToAction(nameof(Index), new { status = "Completed" });
        }
    }
}