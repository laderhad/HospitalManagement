using HospitalManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalManagement.Infrastructure.Data.Configurations;

public class ExaminationConfiguration : IEntityTypeConfiguration<Examination>
{
    public void Configure(EntityTypeBuilder<Examination> builder)
    {
        builder.Property(e => e.Diagnosis).HasMaxLength(500).IsRequired();
        builder.Property(e => e.Treatment).HasMaxLength(500).IsRequired();

        builder.HasOne(e => e.Appointment)
            .WithMany()
            .HasForeignKey(e => e.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.Prescriptions)
            .WithOne(p => p.Examination)
            .HasForeignKey(p => p.ExaminationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.LabRequests)
            .WithOne(lr => lr.Examination)
            .HasForeignKey(lr => lr.ExaminationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasKey(e => e.Id);
    }
}
