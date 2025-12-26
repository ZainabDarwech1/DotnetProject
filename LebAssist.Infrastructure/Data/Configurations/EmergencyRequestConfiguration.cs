using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LebAssist.Infrastructure.Data.Configurations
{
    public class EmergencyRequestConfiguration : IEntityTypeConfiguration<EmergencyRequest>
    {
        public void Configure(EntityTypeBuilder<EmergencyRequest> builder)
        {
            builder.HasKey(er => er.EmergencyRequestId);

            builder.HasIndex(er => er.ClientId);
            builder.HasIndex(er => er.ProviderId);
            builder.HasIndex(er => er.ServiceId);
            builder.HasIndex(er => er.Status);

            builder.Property(er => er.Latitude).HasColumnType("decimal(9,6)");
            builder.Property(er => er.Longitude).HasColumnType("decimal(9,6)");

            // Relationship: Emergency belongs to Client (requester)
            builder.HasOne(er => er.Client)
                   .WithMany(c => c.ClientEmergencies)
                   .HasForeignKey(er => er.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Emergency belongs to Provider (optional, null until accepted)
            builder.HasOne(er => er.Provider)
                   .WithMany(c => c.ProviderEmergencies)
                   .HasForeignKey(er => er.ProviderId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Emergency belongs to Service
            builder.HasOne(er => er.Service)
                   .WithMany(s => s.EmergencyRequests)
                   .HasForeignKey(er => er.ServiceId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}