using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class PatientConfiguration : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.PatientCode)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.HasIndex(p => p.PatientCode)
                   .IsUnique();

            builder.Property(p => p.FullName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.IdentityNumber)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Property(p => p.IdentityType)  // Enum Conversions to string for better readability in the database
                   .HasConversion<string>();

            builder.Property(p => p.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(15);

            builder.Property(p => p.Email)
                   .HasMaxLength(100);

            builder.Property(p => p.NationalityCode)
                   .IsRequired()
                   .HasMaxLength(50);

            builder.Property(p => p.Gender)  // Enum Conversions to string for better readability in the database
                   .HasConversion<string>();

            builder.Property(p => p.ChronicConditions)  // Enum Conversions to string for better readability in the database
                   .HasConversion<string>();

            builder.Property(p => p.OtherConditions)
                   .HasMaxLength(500);

            // Global Query Filter to exclude soft-deleted records
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
