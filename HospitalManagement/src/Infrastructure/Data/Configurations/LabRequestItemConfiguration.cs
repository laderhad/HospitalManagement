using HospitalManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalManagement.Infrastructure.Data.Configurations;

public class LabRequestItemConfiguration : IEntityTypeConfiguration<LabRequestItem>
{
    public void Configure(EntityTypeBuilder<LabRequestItem> builder)
    {
        builder.Property(lri => lri.TestName).HasMaxLength(200).IsRequired();

        builder.HasOne(lri => lri.LabRequest)
            .WithMany(lr => lr.Items)
            .HasForeignKey(lri => lri.LabRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(lri => lri.Results)
            .WithOne(lr => lr.LabRequestItem)
            .HasForeignKey(lr => lr.LabRequestItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(lri => lri.Results).UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasKey(lri => lri.Id);
    }
}
