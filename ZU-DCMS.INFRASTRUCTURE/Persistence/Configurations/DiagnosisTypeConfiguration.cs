using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class DiagnosisTypeConfiguration : IEntityTypeConfiguration<DiagnosisType>
    {
        public void Configure(EntityTypeBuilder<DiagnosisType> builder)
        {
            builder.HasKey(d => d.Id);

            builder.Property(d => d.Code)
                   .IsRequired()
                   .HasMaxLength(20);

            builder.HasIndex(d => d.Code)
                   .IsUnique();

            builder.Property(d => d.NameAr)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(d => d.NameEn)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.HasQueryFilter(d => !d.IsDeleted);

            builder.HasData(GetSeedData());
        }

        private static IEnumerable<DiagnosisType> GetSeedData()
        {
            var dt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return new DiagnosisType[]
            {
                new() { Id=1,  Code="CARIES",     NameAr="تسوس",                NameEn="Dental Caries",           CreatedAt=dt },
                new() { Id=2,  Code="PULPITIS",   NameAr="التهاب لب",           NameEn="Pulpitis",                CreatedAt=dt },
                new() { Id=3,  Code="PULP_NEC",   NameAr="نخر اللب",            NameEn="Pulp Necrosis",           CreatedAt=dt },
                new() { Id=4,  Code="PERI_ABS",   NameAr="خراج ذروي",           NameEn="Periapical Abscess",      CreatedAt=dt },
                new() { Id=5,  Code="GINGIVITIS", NameAr="التهاب لثة",          NameEn="Gingivitis",              CreatedAt=dt },
                new() { Id=6,  Code="PERIODONT",  NameAr="أمراض اللثة",         NameEn="Periodontitis",           CreatedAt=dt },
                new() { Id=7,  Code="IMPACTED",   NameAr="سن معشم",             NameEn="Impacted Tooth",          CreatedAt=dt },
                new() { Id=8,  Code="ROOT_REM",   NameAr="بقايا جذر",           NameEn="Root Remnants",           CreatedAt=dt },
                new() { Id=9,  Code="ABSCESS",    NameAr="خراج",                NameEn="Abscess",                 CreatedAt=dt },
                new() { Id=10, Code="FRACTURE",   NameAr="كسر في السن",         NameEn="Tooth Fracture",          CreatedAt=dt },
                new() { Id=11, Code="MISSING",    NameAr="سن مفقود",            NameEn="Missing Tooth",           CreatedAt=dt },
                new() { Id=12, Code="MALOCCLUS",  NameAr="سوء الإطباق",         NameEn="Malocclusion",            CreatedAt=dt },
                new() { Id=13, Code="EARLY_CAR",  NameAr="تسوس مبكر للأطفال",   NameEn="Early Childhood Caries",  CreatedAt=dt },
                new() { Id=14, Code="GINGIVAL_H", NameAr="تضخم اللثة",          NameEn="Gingival Hyperplasia",    CreatedAt=dt },
                new() { Id=15, Code="MOBILITY",   NameAr="تحرك الأسنان",        NameEn="Tooth Mobility",          CreatedAt=dt },
            };
        }
    }
}
