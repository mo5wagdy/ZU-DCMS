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

            // __ Ensure ApplicationUserId is unique to maintain one-to-one relationship with ApplicationUser with index for performance __ //
            builder.HasIndex(s => s.ApplicationUserId)
                   .IsUnique();

            // __ Global Query Filter to exclude soft-deleted records __ //
            builder.HasQueryFilter(s => !s.IsDeleted);

            // ____________ Relationships ____________ //
            builder.HasOne(s => s.ActiveTerm)
                   .WithMany()
                   .HasForeignKey(s => s.ActiveTermId)
                   .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
