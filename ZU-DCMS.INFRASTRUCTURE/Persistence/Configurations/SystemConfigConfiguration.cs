using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class SystemConfigConfiguration : IEntityTypeConfiguration<SystemConfig>
    {
        public void Configure(EntityTypeBuilder<SystemConfig> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.Key)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasIndex(s => s.Key)
                   .IsUnique();

            builder.Property(s => s.Value)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(s => s.Description)
                   .HasMaxLength(500);

            // Global Query Filter to exclude soft-deleted records
            builder.HasQueryFilter(s => !s.IsDeleted);

            // Seed Default Config
            builder.HasData(
                new SystemConfig
                {
                    Id = 1,
                    Key = "MAX_DAILY_PATIENTS",
                    Value = "200",
                    Description = "أقصى عدد مرضى في اليوم",
                    UpdatedByAdminId = "system"
                },
                new SystemConfig
                {
                    Id = 2,
                    Key = "MAX_NEW_PER_SESSION",
                    Value = "25",
                    Description = "أقصى عدد مرضى جدد في السكشن",
                    UpdatedByAdminId = "system"
                },
                new SystemConfig
                {
                    Id = 3,
                    Key = "MAX_FOLLOWUP_PER_SESSION",
                    Value = "25",
                    Description = "أقصى عدد مرضى متابعة في السكشن",
                    UpdatedByAdminId = "system"
                },
                new SystemConfig
                {
                    Id = 4,
                    Key = "DIAGNOSIS_FEE",
                    Value = "300",
                    Description = "سعر كشف التشخيص",
                    UpdatedByAdminId = "system"
                },
                new SystemConfig
                {
                    Id = 5,
                    Key = "SESSION_TIMES",
                    Value = "09:00,11:00,13:00,15:00",
                    Description = "مواعيد بداية السكاشن",
                    UpdatedByAdminId = "system"
                },
                new SystemConfig
                {
                    Id = 6,
                    Key = "WORKING_DAYS",
                    Value = "0,1,2,3,4,6",
                    Description = "أيام العمل (0=الأحد ... 6=السبت)",
                    UpdatedByAdminId = "system"
                }
            );
        }
    }
}
