using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalManagement.Infrastructure.Data.Configurations;

public class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.Property(d => d.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(d => d.LastName).HasMaxLength(100).IsRequired();
        builder.Property(d => d.ContactNumber)
            .HasConversion(contactNumber => contactNumber.Value, value => ContactNumber.From(value))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(d => d.ApplicationUserId).HasMaxLength(450).IsRequired();
        builder.Property(d => d.Specialty).HasMaxLength(100);
        builder.Property(d => d.Title).HasMaxLength(100);
        builder.Property(d => d.IsActive).IsRequired();

        builder.HasIndex(d => d.ApplicationUserId).IsUnique();

        builder.HasOne(d => d.Department)
            .WithMany(dep => dep.Doctors)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasKey(d => d.Id);
    }
}
