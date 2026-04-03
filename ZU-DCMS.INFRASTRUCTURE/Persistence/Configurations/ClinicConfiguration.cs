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

            // Global Query Filter to exclude soft-deleted records
            builder.HasQueryFilter(c => !c.IsDeleted);

            // Seed Data for Clinics with MaxDailyPatients
            builder.HasData
            (
                new Clinic { Id = 1, Name = "عيادات التشخيص", Code = "DIAG", MaxDailyPatients = 200, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 2, Name = "عيادات حشو العصب", Code = "ENDO", MaxDailyPatients = 50, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 3, Name = "عيادات الجراحة", Code = "SURG", MaxDailyPatients = 50, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 4, Name = "عيادات طب الفم واللثة", Code = "PERIO", MaxDailyPatients = 50, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 5, Name = "عيادات الحشو العادي", Code = "REST", MaxDailyPatients = 50, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 6, Name = "عيادات الأطفال", Code = "PED", MaxDailyPatients = 50, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 7, Name = "التركيبات الثابتة", Code = "FIX", MaxDailyPatients = 50, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Clinic { Id = 8, Name = "التركيبات المتحركة", Code = "REM", MaxDailyPatients = 50, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}
