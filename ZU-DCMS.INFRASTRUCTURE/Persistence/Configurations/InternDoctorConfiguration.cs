using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class InternDoctorConfiguration : IEntityTypeConfiguration<InternDoctor>
    {
        public void Configure(EntityTypeBuilder<InternDoctor> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.DoctorCode)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.HasIndex(d => d.DoctorCode)
                   .IsUnique();

            builder.Property(d => d.FullName)
                   .IsRequired()
                   .HasMaxLength(100);

            // Assuming ApplicationUserId is a foreign key to the AspNetUsers table with indexing for performance
            builder.HasIndex(d => d.ApplicationUserId)
                   .IsUnique();

            // Global Query Filter to exclude soft-deleted records
            builder.HasQueryFilter(d => !d.IsDeleted);
        }
    }
}
