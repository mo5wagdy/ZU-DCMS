using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class DiagnosisRecordConfiguration : IEntityTypeConfiguration<DiagnosisRecord>
    {
        public void Configure(EntityTypeBuilder<DiagnosisRecord> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Complaint)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(d => d.Notes)
                   .HasMaxLength(1000);

            // __ Global Query Filter to exclude soft-deleted records __ //
            builder.HasQueryFilter(d => !d.IsDeleted);

            // ____________ Relationships ____________ //
            builder.HasOne(d => d.Clinic)
                   .WithMany(c => c.DiagnosisRecords)
                   .HasForeignKey(d => d.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.InternDoctor)
                   .WithMany(i => i.DiagnosisRecords)
                   .HasForeignKey(d => d.InternDoctorId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.CaseAssignment)
                   .WithOne(c => c.DiagnosisRecord)
                   .HasForeignKey<CaseAssignment>(c => c.DiagnosisRecordId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
