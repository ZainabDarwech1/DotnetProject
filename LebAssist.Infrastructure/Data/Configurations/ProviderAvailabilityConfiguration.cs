using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LebAssist.Infrastructure.Data.Configurations
{
    public class ProviderAvailabilityConfiguration : IEntityTypeConfiguration<ProviderAvailability>
    {
        public void Configure(EntityTypeBuilder<ProviderAvailability> builder)
        {
            builder.HasKey(pa => pa.AvailabilityId);

            // One-to-One: Each provider has one availability record
            builder.HasIndex(pa => pa.ClientId).IsUnique();

            builder.Property(pa => pa.CurrentLatitude).HasColumnType("decimal(9,6)");
            builder.Property(pa => pa.CurrentLongitude).HasColumnType("decimal(9,6)");

            // Relationship: Availability belongs to Provider
            builder.HasOne(pa => pa.Provider)
                   .WithOne(c => c.Availability)
                   .HasForeignKey<ProviderAvailability>(pa => pa.ClientId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}