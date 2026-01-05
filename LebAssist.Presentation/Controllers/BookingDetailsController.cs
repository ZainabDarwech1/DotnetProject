using LebAssist.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LebAssist.Presentation.Controllers
{
    [Authorize]
    public class BookingDetailsController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IClientService _clientService;

        public BookingDetailsController(
            IBookingService bookingService,
            IClientService clientService)
        {
            _bookingService = bookingService;
            _clientService = clientService;
        }

        public async Task<IActionResult> Details(int id)
        {
            var booking = await _bookingService.GetBookingDetailsAsync(id);
            if (booking == null) return NotFound();
            return View(booking);
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
            return RedirectToAction("Details", new { id = bookingId });
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
            return RedirectToAction("Details", new { id = bookingId });
        }

        [Authorize(Roles = "Provider")]
        [HttpPost]
        public async Task<IActionResult> Start(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            await _bookingService.StartBookingAsync(bookingId, profile.ClientId);
            return RedirectToAction("Details", new { id = bookingId });
        }

        [Authorize(Roles = "Provider")]
        [HttpPost]
        public async Task<IActionResult> Complete(int bookingId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            await _bookingService.CompleteBookingAsync(bookingId, profile.ClientId);
            return RedirectToAction("Details", new { id = bookingId });
        }
    }
}