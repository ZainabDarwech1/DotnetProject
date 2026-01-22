using Domain.Enums;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using LebAssist.Presentation.ViewModels.Report;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LebAssist.Presentation.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IClientService _clientService;
        private readonly IBookingService _bookingService;

        public ReportController(
            IReportService reportService,
            IClientService clientService,
            IBookingService bookingService)
        {
            _reportService = reportService;
            _clientService = clientService;
            _bookingService = bookingService;
        }

        // GET: /Report/MyReports
        public async Task<IActionResult> MyReports()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _clientService.GetProfileAsync(userId!);
            
            var reports = await _reportService.GetClientReportsAsync(profile!.ClientId);
            
            return View(new MyReportsViewModel { Reports = reports.ToList() });
        }

        // GET: /Report/Create
        public async Task<IActionResult> Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _clientService.GetProfileAsync(userId!);

            var model = new CreateReportViewModel();

            // Get client's completed bookings
            var bookings = await _bookingService.GetClientBookingsAsync(profile!.ClientId, BookingStatus.Completed);
            model.MyBookings = bookings
                .Select(b => new BookingOption
                {
                    BookingId = b.BookingId,
                    ProviderName = b.ProviderName,
                    ServiceName = b.ServiceName,
                    BookingDate = b.ScheduledDateTime
                })
                .ToList();

            // Get all providers
            var allClients = await _clientService.GetAllClientsAsync();
            model.AvailableProviders = allClients
                .Where(c => c.IsProvider)
                .Select(c => new ProviderOption
                {
                    ProviderId = c.ClientId,
                    ProviderName = $"{c.FirstName} {c.LastName}"
                })
                .ToList();

            return View(model);
        }

        // POST: /Report/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateReportViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var profile = await _clientService.GetProfileAsync(userId!);

            try
            {
                var dto = new CreateReportDto
                {
                    BookingId = model.BookingId,
                    ProviderId = model.ProviderId,
                    Reason = model.Reason,
                    Description = model.Description
                };

                var reportId = await _reportService.CreateReportAsync(profile!.ClientId, dto);
                
                TempData["Success"] = "Report submitted successfully";
                return RedirectToAction(nameof(MyReports));
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Create));
            }
        }

        // GET: /Report/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var report = await _reportService.GetReportByIdAsync(id);
            if (report == null)
                return NotFound();

            return View(report);
        }
    }
}
