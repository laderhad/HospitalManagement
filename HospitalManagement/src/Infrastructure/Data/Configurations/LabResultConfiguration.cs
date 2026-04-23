using HospitalManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalManagement.Infrastructure.Data.Configurations;

public class LabResultConfiguration : IEntityTypeConfiguration<LabResult>
{
    public void Configure(EntityTypeBuilder<LabResult> builder)
    {
        builder.Property(lr => lr.ResultValue).HasMaxLength(100).IsRequired();
        builder.Property(lr => lr.Units).HasMaxLength(50).IsRequired();
        builder.Property(lr => lr.ReferenceRange).HasMaxLength(100).IsRequired();
        builder.Property(lr => lr.Notes).HasMaxLength(500);
        builder.Property(lr => lr.ResultDate).IsRequired();

        builder.HasOne(lr => lr.LabRequestItem)
            .WithMany(lri => lri.Results)
            .HasForeignKey(lr => lr.LabRequestItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(lr => lr.Id);
    }
}
