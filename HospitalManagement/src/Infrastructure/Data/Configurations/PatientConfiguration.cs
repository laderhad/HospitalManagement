using HospitalManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HospitalManagement.Infrastructure.Data.Configurations;

public class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.Property(p => p.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.LastName).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Gender).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(p => p.ContactNumber).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Address).HasMaxLength(500);

        builder.HasKey(p => p.Id);
    }
}
