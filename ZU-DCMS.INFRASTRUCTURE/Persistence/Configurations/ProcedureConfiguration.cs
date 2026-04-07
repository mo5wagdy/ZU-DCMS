using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class ProcedureConfiguration : IEntityTypeConfiguration<Procedure>
    {
        public void Configure(EntityTypeBuilder<Procedure> builder)
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Code)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.HasIndex(p => p.Code)
                   .IsUnique();

            builder.Property(p => p.NameAr)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.NameEn)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasData(GetSeedData());

            // __ Global Query Filter to exclude soft-deleted records __ //

            builder.HasQueryFilter(p => !p.IsDeleted);

            // ____________ Relationships ____________ //
            builder.HasOne(p => p.Clinic)
                   .WithMany(c => c.Procedures)
                   .HasForeignKey(p => p.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);

        }

        // __ Seed data for Procedure __ //
        private static IEnumerable<Procedure> GetSeedData() =>
        [
            // حشو عادي
            new() { Id=1, ClinicId=5, Code="COMP_FILL",  NameAr="حشو كومبوزيت",    NameEn="Composite Filling",  CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            new() { Id=2, ClinicId=5, Code="AMAL_FILL",  NameAr="حشو أملغم",       NameEn="Amalgam Filling",    CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            // حشو عصب
            new() { Id=3, ClinicId=2, Code="RCT",        NameAr="حشو عصب",         NameEn="Root Canal",         CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            new() { Id=4, ClinicId=2, Code="PULPOTOMY",  NameAr="بتر لب",          NameEn="Pulpotomy",          CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            // جراحة
            new() { Id=5, ClinicId=3, Code="EXTRACT",    NameAr="خلع سن",          NameEn="Extraction",         CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            new() { Id=6, ClinicId=3, Code="SURG_EXT",   NameAr="خلع جراحي",       NameEn="Surgical Extraction",CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            // فم ولثة
            new() { Id=7, ClinicId=4, Code="SCALING",    NameAr="تنظيف جير",       NameEn="Scaling",            CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            new() { Id=8, ClinicId=4, Code="ROOT_PLAN",  NameAr="تنعيم جذور",      NameEn="Root Planing",       CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
        ];
    }
}
