using Domain.Enums;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LebAssist.Presentation.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: /Admin/Reports?status=Pending
        public async Task<IActionResult> Index(string? status)
        {
            var reports = await _reportService.GetAllReportsAsync(status);
            ViewBag.ActiveStatus = status ?? "All";
            return View(reports);
        }

        // GET: /Admin/Reports/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var report = await _reportService.GetReportByIdAsync(id);
            if (report == null)
                return NotFound();

            return View(report);
        }

        // POST: /Admin/Reports/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int reportId, string status, string? adminNotes)
        {
            if (!Enum.TryParse<ReportStatus>(status, out var reportStatus))
            {
                TempData["Error"] = "Invalid status value";
                return RedirectToAction(nameof(Details), new { id = reportId });
            }

            var dto = new UpdateReportStatusDto
            {
                ReportId = reportId,
                Status = reportStatus,
                AdminNotes = adminNotes
            };

            var success = await _reportService.UpdateReportStatusAsync(dto, User.Identity!.Name!);

            if (success)
            {
                TempData["Success"] = $"Report status updated to {reportStatus}";
            }
            else
            {
                TempData["Error"] = "Failed to update report status";
            }

            return RedirectToAction(nameof(Details), new { id = reportId });
        }
    }
}
