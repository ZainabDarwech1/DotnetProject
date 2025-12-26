using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LebAssist.Infrastructure.Data.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.BookingId);

            builder.HasIndex(b => b.ClientId);
            builder.HasIndex(b => b.ProviderId);
            builder.HasIndex(b => b.ServiceId);
            builder.HasIndex(b => b.Status);
            builder.HasIndex(b => b.ScheduledDateTime);

            builder.Property(b => b.Latitude).HasColumnType("decimal(9,6)");
            builder.Property(b => b.Longitude).HasColumnType("decimal(9,6)");

            // Relationship: Booking belongs to Client (as requester)
            builder.HasOne(b => b.Client)
                   .WithMany(c => c.ClientBookings)
                   .HasForeignKey(b => b.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Booking belongs to Provider
            builder.HasOne(b => b.Provider)
                   .WithMany(c => c.ProviderBookings)
                   .HasForeignKey(b => b.ProviderId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Booking belongs to Service
            builder.HasOne(b => b.Service)
                   .WithMany(s => s.Bookings)
                   .HasForeignKey(b => b.ServiceId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}