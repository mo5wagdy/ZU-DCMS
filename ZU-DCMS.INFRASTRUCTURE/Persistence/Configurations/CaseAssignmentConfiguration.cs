using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class CaseAssignmentConfiguration : IEntityTypeConfiguration<CaseAssignment>
    {
        public void Configure(EntityTypeBuilder<CaseAssignment> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Notes)
                   .HasMaxLength(500);

            // __ Enum Conversions to string for better readability in the database __ //
            builder.Property(c => c.Status)
                   .HasConversion<string>();

            // __ Global Query Filter to exclude soft-deleted records __ //
            builder.HasQueryFilter(c => !c.IsDeleted);

            // ____________ Relationships ____________ //
            builder.HasOne(c => c.Student)
                   .WithMany(s => s.CaseAssignments)
                   .HasForeignKey(c => c.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Clinic)
                   .WithMany(cl => cl.CaseAssignments)
                   .HasForeignKey(c => c.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.AssignedByIntern)
                   .WithMany(i => i.CaseAssignments)
                   .HasForeignKey(c => c.AssignedByInternId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Sessions)
                   .WithOne(s => s.CaseAssignment)
                   .HasForeignKey(s => s.CaseAssignmentId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
