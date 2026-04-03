using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.StudentCode)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.HasIndex(s => s.StudentCode)
                   .IsUnique();

            builder.Property(s => s.FullName)
                   .IsRequired()
                   .HasMaxLength(100);

            // Ensure ApplicationUserId is unique to maintain one-to-one relationship with ApplicationUser with index for performance
            builder.HasIndex(s => s.ApplicationUserId)
                   .IsUnique();

            // Global Query Filter to exclude soft-deleted records
            builder.HasQueryFilter(s => !s.IsDeleted);

            // Relationships
            builder.HasOne(s => s.ActiveTerm)
                   .WithMany()
                   .HasForeignKey(s => s.ActiveTermId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
