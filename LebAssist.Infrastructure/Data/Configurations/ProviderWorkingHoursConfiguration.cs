using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LebAssist.Infrastructure.Data.Configurations
{
    public class ProviderWorkingHoursConfiguration : IEntityTypeConfiguration<ProviderWorkingHours>
    {
        public void Configure(EntityTypeBuilder<ProviderWorkingHours> builder)
        {
            builder.HasKey(pwh => pwh.WorkingHourId);

            builder.HasIndex(pwh => pwh.ClientId);
            builder.HasIndex(pwh => pwh.ServiceId);

            // Relationship: WorkingHours belongs to Provider
            builder.HasOne(pwh => pwh.Provider)
                   .WithMany(c => c.WorkingHours)
                   .HasForeignKey(pwh => pwh.ClientId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Relationship: WorkingHours belongs to Service
            builder.HasOne(pwh => pwh.Service)
                   .WithMany(s => s.WorkingHours)
                   .HasForeignKey(pwh => pwh.ServiceId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}