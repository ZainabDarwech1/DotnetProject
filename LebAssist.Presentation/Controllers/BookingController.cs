using Domain.Enums;
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
        private readonly IProviderService _providerService;
        private readonly IServiceService _serviceService;

        public BookingController(
            IBookingService bookingService,
            IClientService clientService,
            IProviderService providerService,
            IServiceService serviceService)
        {
            _bookingService = bookingService;
            _clientService = clientService;
            _providerService = providerService;
            _serviceService = serviceService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int providerId, int serviceId)
        {
            var provider = await _clientService.GetClientByIdAsync(providerId);
            var service = await _serviceService.GetServiceByIdAsync(serviceId);
            if (provider == null || service == null) return NotFound();

            // Get working hours for this provider & service
            var hours = await _providerService.GetServiceWorkingHoursAsync(providerId, serviceId);
            var model = new CreateBookingViewModel
            {
                ProviderId = providerId,
                ServiceId = serviceId,
                BookingDateTime = DateTime.Now.AddDays(1),
                Latitude = 33.8938,
                Longitude = 35.5018
            };

            ViewBag.Provider = provider;
            ViewBag.Service = service;
            ViewBag.WorkingHours = hours;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBookingViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // repopulate view data
                var provider = await _clientService.GetClientByIdAsync(model.ProviderId);
                var service = await _serviceService.GetServiceByIdAsync(model.ServiceId);
                var hours = await _providerService.GetServiceWorkingHoursAsync(model.ProviderId, model.ServiceId);
                ViewBag.Provider = provider;
                ViewBag.Service = service;
                ViewBag.WorkingHours = hours;

                return View(model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            // Validate selected datetime falls within provider working hours
            var serviceHoursList = await _providerService.GetServiceWorkingHoursAsync(model.ProviderId, model.ServiceId);
            var serviceHours = serviceHoursList.FirstOrDefault();

            if (serviceHours == null || !serviceHours.DaySchedules.Any())
            {
                ModelState.AddModelError(string.Empty, "The provider has not set working hours for this service.");
                var provider = await _clientService.GetClientByIdAsync(model.ProviderId);
                var service = await _serviceService.GetServiceByIdAsync(model.ServiceId);
                ViewBag.Provider = provider;
                ViewBag.Service = service;
                ViewBag.WorkingHours = serviceHoursList;
                return View(model);
            }

            var scheduledDay = (int)model.BookingDateTime.DayOfWeek; // 0=Sunday
            var daySchedule = serviceHours.DaySchedules.FirstOrDefault(d => d.DayOfWeek == scheduledDay);

            if (daySchedule == null)
            {
                ModelState.AddModelError(string.Empty, "Provider is not available at the selected day/time.");
                var provider = await _clientService.GetClientByIdAsync(model.ProviderId);
                var service = await _serviceService.GetServiceByIdAsync(model.ServiceId);
                ViewBag.Provider = provider;
                ViewBag.Service = service;
                ViewBag.WorkingHours = serviceHoursList;
                return View(model);
            }

            var timeOfDay = model.BookingDateTime.TimeOfDay;
            if (timeOfDay < daySchedule.StartTime || timeOfDay > daySchedule.EndTime)
            {
                ModelState.AddModelError(string.Empty, $"Please choose a time between {daySchedule.StartTime} and {daySchedule.EndTime}.");
                var provider = await _clientService.GetClientByIdAsync(model.ProviderId);
                var service = await _serviceService.GetServiceByIdAsync(model.ServiceId);
                ViewBag.Provider = provider;
                ViewBag.Service = service;
                ViewBag.WorkingHours = serviceHoursList;
                return View(model);
            }

            await _bookingService.CreateBookingAsync(profile.ClientId, model.ToDto());
            TempData["Success"] = "Booking created successfully!";
            return RedirectToAction("MyBookings");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int bookingId, string? reason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            var result = await _bookingService.CancelBookingByClientAsync(bookingId, profile.ClientId, reason ?? string.Empty);
            if (result)
                TempData["Success"] = "Booking cancelled successfully.";
            else
                TempData["Error"] = "Failed to cancel booking. It may already be processed.";

            return RedirectToAction(nameof(MyBookings));
        }

        public async Task<IActionResult> MyBookings(BookingStatus? tab = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var profile = await _clientService.GetProfileAsync(userId);
            if (profile == null) return Unauthorized();

            var bookings = await _bookingService
                .GetClientBookingsAsync(profile.ClientId, tab);

            ViewBag.ActiveTab = tab?.ToString() ?? "All";
            return View(bookings);
        }

    }
}