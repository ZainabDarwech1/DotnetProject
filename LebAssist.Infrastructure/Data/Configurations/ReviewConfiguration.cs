using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LebAssist.Infrastructure.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> builder)
        {
            builder.HasKey(r => r.ReviewId);

            // One-to-One: Each booking has one review
            builder.HasIndex(r => r.BookingId).IsUnique();
            builder.HasIndex(r => r.ClientId);
            builder.HasIndex(r => r.ProviderId);

            // Relationship: Review belongs to Booking (One-to-One)
            builder.HasOne(r => r.Booking)
                   .WithOne(b => b.Review)
                   .HasForeignKey<Review>(r => r.BookingId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relationship: Review written by Client
            builder.HasOne(r => r.Client)
                   .WithMany(c => c.ReviewsWritten)
                   .HasForeignKey(r => r.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Review for Provider
            builder.HasOne(r => r.Provider)
                   .WithMany(c => c.ReviewsReceived)
                   .HasForeignKey(r => r.ProviderId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}