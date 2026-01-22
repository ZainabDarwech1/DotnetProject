using Domain.Enums;

namespace LebAssist.Application.DTOs
{
    public class CreateReportDto
    {
        public int? BookingId { get; set; }
        public int? ProviderId { get; set; }
        public ReportReason Reason { get; set; }
        public string Description { get; set; } = string.Empty;
    }

    public class ReportDto
    {
        public int ReportId { get; set; }
        public int ReporterId { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public string? ReporterPhone { get; set; }
        public int ReportedProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public int? BookingId { get; set; }
        public string? ServiceName { get; set; }
        public ReportReason Reason { get; set; }
        public string Description { get; set; } = string.Empty;
        public ReportStatus Status { get; set; }
        public DateTime ReportDate { get; set; }
        public string? ResolvedBy { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string? AdminNotes { get; set; }
    }

    public class UpdateReportStatusDto
    {
        public int ReportId { get; set; }
        public ReportStatus Status { get; set; }
        public string? AdminNotes { get; set; }
    }
}
