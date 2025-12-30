using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LebAssist.Infrastructure.Data.Configurations
{
    public class ProviderServiceConfiguration : IEntityTypeConfiguration<ProviderServiceEntity>
    {
        public void Configure(EntityTypeBuilder<ProviderServiceEntity> builder)
        {
            builder.HasKey(ps => ps.ProviderServiceId);

            builder.HasIndex(ps => ps.ClientId);
            builder.HasIndex(ps => ps.ServiceId);
            builder.HasIndex(ps => ps.IsActive);

            // Unique: Provider can offer same service only once
            builder.HasIndex(ps => new { ps.ClientId, ps.ServiceId }).IsUnique();

            // Relationship: ProviderService belongs to Client (Provider)
            builder.HasOne(ps => ps.Provider)
                   .WithMany(c => c.ProviderServices)
                   .HasForeignKey(ps => ps.ClientId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relationship: ProviderService belongs to Service
            builder.HasOne(ps => ps.Service)
                   .WithMany(s => s.ProviderServices)
                   .HasForeignKey(ps => ps.ServiceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(ps => ps.PricePerHour).HasColumnType("decimal(10,2)");
        }
    }
}