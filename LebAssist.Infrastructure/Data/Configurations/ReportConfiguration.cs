using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LebAssist.Infrastructure.Data.Configurations
{
    public class ReportConfiguration : IEntityTypeConfiguration<Report>
    {
        public void Configure(EntityTypeBuilder<Report> builder)
        {
            builder.HasKey(r => r.ReportId);

            builder.HasIndex(r => r.ReporterId);
            builder.HasIndex(r => r.ReportedProviderId);
            builder.HasIndex(r => r.Status);

            // Relationship: Report submitted by Reporter
            builder.HasOne(r => r.Reporter)
                   .WithMany(c => c.ReportsSubmitted)
                   .HasForeignKey(r => r.ReporterId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Report against Provider
            builder.HasOne(r => r.ReportedProvider)
                   .WithMany(c => c.ReportsReceived)
                   .HasForeignKey(r => r.ReportedProviderId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Report optionally linked to Booking
            builder.HasOne(r => r.Booking)
                   .WithMany(b => b.Reports)
                   .HasForeignKey(r => r.BookingId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}