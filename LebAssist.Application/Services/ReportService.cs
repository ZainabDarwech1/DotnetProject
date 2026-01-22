using Domain.Entities;
using Domain.Enums;
using Domain.Interfaces;
using LebAssist.Application.DTOs;
using LebAssist.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LebAssist.Application.Services
{
    public class ReportService : IReportService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IUnitOfWork unitOfWork, ILogger<ReportService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<int> CreateReportAsync(int clientId, CreateReportDto dto)
        {
            int providerId;

            // If BookingId is provided, get provider from booking
            if (dto.BookingId.HasValue)
            {
                var booking = await _unitOfWork.Bookings.GetByIdAsync(dto.BookingId.Value);
                if (booking == null)
                    throw new ArgumentException("Booking not found");
                
                providerId = booking.ProviderId;
            }
            // Otherwise, use the provided ProviderId
            else if (dto.ProviderId.HasValue)
            {
                providerId = dto.ProviderId.Value;
            }
            else
            {
                throw new ArgumentException("Either BookingId or ProviderId must be provided");
            }

            var report = new Report
            {
                ReporterId = clientId,
                ReportedProviderId = providerId,
                BookingId = dto.BookingId,
                Reason = dto.Reason,
                Description = dto.Description,
                Status = ReportStatus.Pending,
                ReportDate = DateTime.UtcNow
            };

            await _unitOfWork.Reports.AddAsync(report);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Report {ReportId} created", report.ReportId);
            return report.ReportId;
        }

        public async Task<IEnumerable<ReportDto>> GetClientReportsAsync(int clientId)
        {
            var reports = await _unitOfWork.Reports.GetByReporterIdAsync(clientId);
            return reports.Select(r => MapToDto(r));
        }

        public async Task<ReportDto?> GetReportByIdAsync(int reportId)
        {
            var r = await _unitOfWork.Reports.GetByIdWithDetailsAsync(reportId);
            if (r == null) return null;
            return MapToDto(r);
        }

        // Admin operations
        public async Task<IEnumerable<ReportDto>> GetAllReportsAsync(string? status = null)
        {
            var reports = await _unitOfWork.Reports.GetAllWithDetailsAsync();

            // Filter by status if provided
            if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReportStatus>(status, true, out var reportStatus))
            {
                reports = reports.Where(r => r.Status == reportStatus);
            }

            return reports.Select(r => MapToDto(r)).OrderByDescending(r => r.ReportDate);
        }

        public async Task<bool> UpdateReportStatusAsync(UpdateReportStatusDto dto, string adminUsername)
        {
            try
            {
                var report = await _unitOfWork.Reports.GetByIdAsync(dto.ReportId);
                if (report == null)
                {
                    _logger.LogWarning("Report {ReportId} not found", dto.ReportId);
                    return false;
                }

                report.Status = dto.Status;
                report.AdminNotes = dto.AdminNotes;
                report.ResolvedBy = adminUsername;

                if (dto.Status == ReportStatus.Resolved || dto.Status == ReportStatus.Dismissed)
                {
                    report.ResolvedDate = DateTime.UtcNow;
                }

                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("Report {ReportId} status updated to {Status} by {Admin}", 
                    dto.ReportId, dto.Status, adminUsername);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating report {ReportId}", dto.ReportId);
                return false;
            }
        }

        private ReportDto MapToDto(Report r)
        {
            return new ReportDto
            {
                ReportId = r.ReportId,
                ReporterId = r.ReporterId,
                ReporterName = $"{r.Reporter.FirstName} {r.Reporter.LastName}",
                ReporterPhone = r.Reporter?.PhoneNumber,
                ReportedProviderId = r.ReportedProviderId,
                ProviderName = $"{r.ReportedProvider.FirstName} {r.ReportedProvider.LastName}",
                BookingId = r.BookingId,
                ServiceName = r.Booking?.Service?.ServiceName,
                Reason = r.Reason,
                Description = r.Description,
                Status = r.Status,
                ReportDate = r.ReportDate,
                ResolvedBy = r.ResolvedBy,
                ResolvedDate = r.ResolvedDate,
                AdminNotes = r.AdminNotes
            };
        }
    }
}
