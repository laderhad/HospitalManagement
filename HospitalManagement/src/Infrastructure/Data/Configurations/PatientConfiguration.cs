using HospitalManagement.Domain.Entities;
using HospitalManagement.Domain.ValueObjects;
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
        builder.Property(p => p.ContactNumber)
            .HasConversion(contactNumber => contactNumber.Value, value => ContactNumber.From(value))
            .HasMaxLength(20)
            .IsRequired();
        builder.Property(p => p.Address)
            .HasConversion(address => address.Value, value => PostalAddress.From(value))
            .HasMaxLength(500)
            .IsRequired();
        builder.Property(p => p.ApplicationUserId).HasMaxLength(450).IsRequired();

        builder.HasIndex(p => p.ApplicationUserId).IsUnique();

        builder.HasKey(p => p.Id);
    }
}
