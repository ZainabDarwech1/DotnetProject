using Domain.Enums;
using LebAssist.Application.DTOs;
using System.ComponentModel.DataAnnotations;

namespace LebAssist.Presentation.ViewModels.Report
{
    public class CreateReportViewModel
    {
        public int? BookingId { get; set; }
        public int? ProviderId { get; set; }

        [Required(ErrorMessage = "Please select a reason")]
        public ReportReason Reason { get; set; }

        [Required(ErrorMessage = "Please describe the issue")]
        [StringLength(2000, MinimumLength = 20)]
        public string Description { get; set; } = string.Empty;

        // For display purposes
        public List<ProviderOption> AvailableProviders { get; set; } = new();
        public List<BookingOption> MyBookings { get; set; } = new();
    }

    public class ProviderOption
    {
        public int ProviderId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
    }

    public class BookingOption
    {
        public int BookingId { get; set; }
        public string ProviderName { get; set; } = string.Empty;
        public string ServiceName { get; set; } = string.Empty;
        public DateTime BookingDate { get; set; }
    }

    public class MyReportsViewModel
    {
        public List<ReportDto> Reports { get; set; } = new();
    }
}
