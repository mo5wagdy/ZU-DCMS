using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class ClinicConfiguration : IEntityTypeConfiguration<Clinic>
    {
        public void Configure(EntityTypeBuilder<Clinic> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Code)
                   .IsRequired()
                   .HasMaxLength(10);

            builder.HasIndex(c => c.Code)
                   .IsUnique();

            builder.Property(c => c.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(c => c.NameAr)
                   .HasMaxLength(100)
                   .HasDefaultValue("");

            builder.Property(c => c.NameEn)
                   .HasMaxLength(100)
                   .HasDefaultValue("");

            // _____________ Academic Year Constraints Configuration _____________ //
            /// <summary>
            /// Configure academic year range constraints.
            /// These ensure students can only be assigned to clinics appropriate for their skill level.
            /// For example: MinAcademicYear = 2 means only 2nd year and above can work in this clinic.
            /// </summary>
            builder.Property(c => c.MinAcademicYear)
                   .IsRequired()
                   .HasDefaultValue(1);

            builder.Property(c => c.MaxAcademicYear)
                   .IsRequired()
                   .HasDefaultValue(4);

            // _____________ Workload Constraint Configuration _____________ //
            /// <summary>
            /// Configure maximum cases per student per clinic.
            /// Default: 3 active cases per student per clinic.
            /// Prevents student overload and ensures case quality.
            /// </summary>
            builder.Property(c => c.MaxCasesPerStudent)
                   .IsRequired()
                   .HasDefaultValue(3);

            // __ Global Query Filter to exclude soft-deleted records __ //
            builder.HasQueryFilter(c => !c.IsDeleted);

            // _____________ Seed Data with Academic Year & Workload Constraints _____________ //
            /// <summary>
            /// Initialize clinics with appropriate academic year ranges:
            /// - DIAG (Diagnosis): 1-4 years (all students can assist with screening)
            /// - ENDO (Endodontics): 2-4 years (requires intermediate skills)
            /// - SURG (Surgery): 3-4 years (requires advanced skills)
            /// - PERIO (Periodontics): 2-4 years
            /// - REST (Restorative): 1-4 years (foundational)
            /// - PED (Pediatrics): 1-4 years (foundational)
            /// - FIX (Fixed Prosthodontics): 2-4 years
            /// - REM (Removable Prosthodontics): 2-4 years
            /// </summary>
            builder.HasData
            (
                new Clinic { Id = 1, Name = "عيادات التشخيص",         NameAr = "عيادات التشخيص",          NameEn = "Diagnosis Clinics",                Code = "DIAG",  MaxDailyPatients = 200, MinAcademicYear = 1, MaxAcademicYear = 4, MaxCasesPerStudent = 5, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 2, Name = "عيادات حشو العصب",       NameAr = "عيادات حشو العصب",        NameEn = "Endodontics Clinics",              Code = "ENDO",  MaxDailyPatients = 50,  MinAcademicYear = 4, MaxAcademicYear = 4, MaxCasesPerStudent = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 3, Name = "عيادات الجراحة",         NameAr = "عيادات الجراحة",          NameEn = "Surgery Clinics",                  Code = "SURG",  MaxDailyPatients = 50,  MinAcademicYear = 3, MaxAcademicYear = 4, MaxCasesPerStudent = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 4, Name = "عيادات طب الفم واللثة",  NameAr = "عيادات طب الفم واللثة",   NameEn = "Periodontics Clinics",             Code = "PERIO", MaxDailyPatients = 50,  MinAcademicYear = 3, MaxAcademicYear = 4, MaxCasesPerStudent = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 5, Name = "عيادات الحشو العادي",    NameAr = "عيادات الحشو العادي",     NameEn = "Restorative Clinics",              Code = "REST",  MaxDailyPatients = 50,  MinAcademicYear = 3, MaxAcademicYear = 4, MaxCasesPerStudent = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 6, Name = "عيادات الأطفال",         NameAr = "عيادات الأطفال",          NameEn = "Pediatric Dentistry Clinics",       Code = "PED",   MaxDailyPatients = 50,  MinAcademicYear = 4, MaxAcademicYear = 4, MaxCasesPerStudent = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 7, Name = "التركيبات الثابتة",      NameAr = "التركيبات الثابتة",       NameEn = "Fixed Prosthodontics Clinics",     Code = "FIX",   MaxDailyPatients = 50,  MinAcademicYear = 4, MaxAcademicYear = 4, MaxCasesPerStudent = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 8, Name = "التركيبات المتحركة",     NameAr = "التركيبات المتحركة",      NameEn = "Removable Prosthodontics Clinics", Code = "REM",   MaxDailyPatients = 50,  MinAcademicYear = 4, MaxAcademicYear = 4, MaxCasesPerStudent = 3, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
