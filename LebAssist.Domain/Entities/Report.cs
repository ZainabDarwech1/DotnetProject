using Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities
{
    public class Report
    {
        [Key]
        public int ReportId { get; set; }

        public int ReporterId { get; set; }

        public int ReportedProviderId { get; set; }

        public int? BookingId { get; set; }

        public ReportReason Reason { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        public ReportStatus Status { get; set; } = ReportStatus.Pending;

        public DateTime ReportDate { get; set; } = DateTime.UtcNow;

        public string? ResolvedBy { get; set; }

        public DateTime? ResolvedDate { get; set; }

        [MaxLength(1000)]
        public string? AdminNotes { get; set; }

        // Navigation Properties
        public virtual Client Reporter { get; set; } = null!;
        public virtual Client ReportedProvider { get; set; } = null!;
        public virtual Booking? Booking { get; set; }
    }
}