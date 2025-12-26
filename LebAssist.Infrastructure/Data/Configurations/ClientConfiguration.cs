using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LebAssist.Infrastructure.Data.Configurations
{
    public class ClientConfiguration : IEntityTypeConfiguration<Client>
    {
        public void Configure(EntityTypeBuilder<Client> builder)
        {
            // Primary Key
            builder.HasKey(c => c.ClientId);

            // Indexes for faster queries
            builder.HasIndex(c => c.AspNetUserId).IsUnique();
            builder.HasIndex(c => c.IsProvider);
            builder.HasIndex(c => c.ProviderStatus);

            // Decimal precision for coordinates
            builder.Property(c => c.Latitude).HasColumnType("decimal(9,6)");
            builder.Property(c => c.Longitude).HasColumnType("decimal(9,6)");
        }
    }
}