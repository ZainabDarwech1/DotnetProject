using LebAssist.Application.Interfaces;
using LebAssist.Presentation.ViewModels.Booking;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LebAssist.Presentation.Controllers
{
    [Authorize(Roles = "Client")]
    public class BookingController : Controller
    {
        private readonly IBookingService _bookingService;
        private readonly IClientService _clientService;

        public BookingController(
            IBookingService bookingService,
            IClientService clientService)
        {
            _bookingService = bookingService;
            _clientService = clientService;
        }

        [HttpGet]
        public IActionResult Create(int providerId, int serviceId)
        {
            var model = new CreateBookingViewModel
            {
                ProviderId = providerId,
                ServiceId = serviceId
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            await _bookingService.CreateBookingAsync(profile.ClientId, model.ToDto());
            TempData["Success"] = "Booking created successfully!";
            return RedirectToAction("Index", "Home");
        }
    }
}