using HospitalManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalManagement.Infrastructure.Data.Configurations;

public class PrescriptionConfiguration : IEntityTypeConfiguration<Prescription>
{
    public void Configure(EntityTypeBuilder<Prescription> builder)
    {
        builder.Property(p => p.MedicationName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Dosage).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Frequency).HasMaxLength(100).IsRequired();
        builder.Property(p => p.DurationDays).IsRequired();
        builder.Property(p => p.Notes).HasMaxLength(500);

        builder.HasOne(p => p.Examination)
            .WithMany(e => e.Prescriptions)
            .HasForeignKey(p => p.ExaminationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(p => p.Id);
    }
}
