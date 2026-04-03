using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class CaseSessionConfiguration : IEntityTypeConfiguration<CaseSession>
    {
        public void Configure(EntityTypeBuilder<CaseSession> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.ProceduresDone)
                   .IsRequired()
                   .HasMaxLength(1000);

            builder.Property(s => s.Notes)
                   .HasMaxLength(500);

            // Global Query Filter to exclude soft-deleted records
            builder.HasQueryFilter(s => !s.IsDeleted);

            // Relationships
            builder.HasOne(s => s.Student)
                   .WithMany()
                   .HasForeignKey(s => s.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
