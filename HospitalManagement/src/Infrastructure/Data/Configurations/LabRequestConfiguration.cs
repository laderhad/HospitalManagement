using HospitalManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalManagement.Infrastructure.Data.Configurations;

public class LabRequestConfiguration : IEntityTypeConfiguration<LabRequest>
{
    public void Configure(EntityTypeBuilder<LabRequest> builder)
    {
        builder.Property(lr => lr.RequestDate).IsRequired();

        builder.HasOne(lr => lr.Examination)
            .WithMany(e => e.LabRequests)
            .HasForeignKey(lr => lr.ExaminationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(lr => lr.Items)
            .WithOne(lri => lri.LabRequest)
            .HasForeignKey(lri => lri.LabRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(lr => lr.Items).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasKey(lr => lr.Id);
    }
}
