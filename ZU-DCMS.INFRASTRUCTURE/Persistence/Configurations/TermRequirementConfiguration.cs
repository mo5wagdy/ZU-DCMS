using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class TermRequirementConfiguration : IEntityTypeConfiguration<TermRequirement>
    {
        public void Configure(EntityTypeBuilder<TermRequirement> builder)
        {
            builder.HasKey(r => r.Id);

            //StudentId + TermId + ClinicId should be unique to prevent duplicate requirements for the same student, term, and clinic
            builder.HasIndex(r => new { r.StudentId, r.TermId, r.ClinicId })
                   .IsUnique();

            // Global Query Filter to exclude soft-deleted records
            builder.HasQueryFilter(r => !r.IsDeleted);

            // Relationships
            builder.HasOne(r => r.Student)
                   .WithMany(s => s.TermRequirements)
                   .HasForeignKey(r => r.StudentId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Term)
                   .WithMany(t => t.TermRequirements)
                   .HasForeignKey(r => r.TermId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Clinic)
                   .WithMany(c => c.TermRequirements)
                   .HasForeignKey(r => r.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
