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
                   .HasDefaultValueSql("'PAT-' + CAST(YEAR(GETDATE()) AS VARCHAR) + '-' + RIGHT('0000' + CAST(NEXT VALUE FOR PatientCodeSeq AS VARCHAR), 4)") // => This sets a default value for PatientCode using a SQL expression that generates a unique code based on the current year and a sequence.
                   .HasMaxLength(20);

            builder.HasIndex(p => p.PatientCode)
                   .IsUnique();

            builder.Property(p => p.FullName)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.IdentityNumber)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.Ignore(p => p.Age); // => Age is a computed property based on DateOfBirth, so we ignore it in EF Core mapping.

            // __ Enum Conversions to string for better readability in the database __ //
            builder.Property(p => p.IdentityType)  
                   .HasConversion<string>();

            builder.Property(p => p.Gender)
                   .HasConversion<string>();

            builder.Property(p => p.ChronicConditions)
                   .HasConversion<string>();
            //________________________________________________________________//

            builder.Property(p => p.PhoneNumber)
                   .IsRequired()
                   .HasMaxLength(15);

            builder.Property(p => p.Email)
                   .HasMaxLength(100);

            builder.Property(p => p.Address)
                   .IsRequired()
                   .HasMaxLength(200);


            builder.Property(p => p.OtherConditions)
                   .HasMaxLength(500);

            // __ Global Query Filter to exclude soft-deleted records __ //
            builder.HasQueryFilter(p => !p.IsDeleted);
        }
    }
}
