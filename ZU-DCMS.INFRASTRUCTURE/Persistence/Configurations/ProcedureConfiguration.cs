using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
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

            builder.HasQueryFilter(p => !p.IsDeleted);

            builder.HasData(GetSeedData());
        }

        private static IEnumerable<Procedure> GetSeedData()
        {
            var dt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return new Procedure[]
            {
                // REST (5)
                new() { Id=1,  Code="COMP_FILL",  NameAr="حشو كومبوزيت",         NameEn="Composite Filling",      CreatedAt=dt },
                new() { Id=2,  Code="GIC_FILL",   NameAr="حشو زجاجي",            NameEn="GIC Filling",            CreatedAt=dt },
                new() { Id=3,  Code="AMAL_FILL",  NameAr="حشو أملغم",            NameEn="Amalgam Filling",        CreatedAt=dt },

                // ENDO (2)
                new() { Id=4,  Code="RCT",        NameAr="حشو عصب",              NameEn="Root Canal Treatment",   CreatedAt=dt },
                new() { Id=5,  Code="PULPECT",    NameAr="استئصال لب",           NameEn="Pulpectomy",             CreatedAt=dt },
                new() { Id=6,  Code="RETREATM",   NameAr="إعادة حشو عصب",        NameEn="RCT Retreatment",        CreatedAt=dt },

                // OS (3)
                new() { Id=7,  Code="EXTRACT",    NameAr="خلع سن",               NameEn="Extraction",             CreatedAt=dt },
                new() { Id=8,  Code="SURG_EXT",   NameAr="خلع جراحي",            NameEn="Surgical Extraction",    CreatedAt=dt },
                new() { Id=9,  Code="INC_DRAIN",  NameAr="فتح وتصريف خراج",      NameEn="Incision & Drainage",    CreatedAt=dt },
                new() { Id=10, Code="BIOPSY",     NameAr="خزعة",                 NameEn="Biopsy",                 CreatedAt=dt },

                // PERIO (4)
                new() { Id=11, Code="SCALING",    NameAr="تنظيف جير",            NameEn="Scaling",                CreatedAt=dt },
                new() { Id=12, Code="ROOT_PLAN",  NameAr="تنعيم جذور",           NameEn="Root Planing",           CreatedAt=dt },
                new() { Id=13, Code="GINGIVECT",  NameAr="استئصال لثة",          NameEn="Gingivectomy",           CreatedAt=dt },

                // PEDO (6)
                new() { Id=14, Code="PULPOTOMY",  NameAr="بتر لب",               NameEn="Pulpotomy",              CreatedAt=dt },
                new() { Id=15, Code="SSC_CROWN",  NameAr="تاج فولاذي للأطفال",   NameEn="Stainless Steel Crown",  CreatedAt=dt },
                new() { Id=16, Code="FLUORIDE",   NameAr="فلورايد",              NameEn="Fluoride Application",   CreatedAt=dt },
                new() { Id=17, Code="SEALANT",    NameAr="حماية أسنان",          NameEn="Fissure Sealant",        CreatedAt=dt },

                // FPD (7)
                new() { Id=18, Code="CROWN_PREP", NameAr="تحضير تاج",            NameEn="Crown Preparation",      CreatedAt=dt },
                new() { Id=19, Code="BRIDGE",     NameAr="جسر أسنان",            NameEn="Bridge",                 CreatedAt=dt },
                new() { Id=20, Code="CEMENT",     NameAr="تثبيت تركيبة",         NameEn="Cementation",            CreatedAt=dt },

                // RPD (8)
                new() { Id=21, Code="COMP_DEN",   NameAr="طقم أسنان كامل",       NameEn="Complete Denture",       CreatedAt=dt },
                new() { Id=22, Code="PART_DEN",   NameAr="طقم أسنان جزئي",       NameEn="Partial Denture",        CreatedAt=dt },
                new() { Id=23, Code="DEN_ADJ",    NameAr="تعديل طقم",            NameEn="Denture Adjustment",     CreatedAt=dt },
            };
        }
    }
}
