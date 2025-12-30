using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LebAssist.Infrastructure.Data.Configurations
{
    public class ReviewConfiguration : IEntityTypeConfiguration<Review>
    {
        public void Configure(EntityTypeBuilder<Review> entity)
        {
            entity.HasKey(r => r.ReviewId);

            // One review per booking
            entity.HasIndex(r => r.BookingId).IsUnique();

            entity.Property(r => r.Rating)
                  .IsRequired();

            entity.Property(r => r.Comment)
                  .HasMaxLength(1000);

            entity.Property(r => r.ReviewDate)
                  .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(r => r.IsVisible)
                  .HasDefaultValue(true);

            entity.Property(r => r.IsAnonymous)
                  .HasDefaultValue(false);

            entity.Property(r => r.AdminModerated)
                  .HasDefaultValue(false);

            // Booking (1) <-> (0..1) Review
            entity.HasOne(r => r.Booking)
                  .WithOne(b => b.Review)
                  .HasForeignKey<Review>(r => r.BookingId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Reviewer (Client) - ReviewsWritten collection
            entity.HasOne(r => r.Client)
                  .WithMany(c => c.ReviewsWritten)
                  .HasForeignKey(r => r.ClientId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Reviewed Provider - ReviewsReceived collection
            entity.HasOne(r => r.Provider)
                  .WithMany(c => c.ReviewsReceived)
                  .HasForeignKey(r => r.ProviderId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Indexes for performance
            entity.HasIndex(r => new { r.ProviderId, r.IsVisible, r.ReviewDate });
            entity.HasIndex(r => r.ClientId);
            entity.HasIndex(r => new { r.IsVisible, r.AdminModerated });
        }
    }
}