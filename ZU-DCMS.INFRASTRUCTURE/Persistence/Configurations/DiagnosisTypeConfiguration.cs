using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
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

            builder.HasData(GetSeedData());

            // __ Global Query Filter to exclude soft-deleted records __ //
            builder.HasQueryFilter(d => !d.IsDeleted);

            // ____________ Relationships ____________ //
            builder.HasOne(d => d.Clinic)
                   .WithMany(c => c.DiagnosisTypes)
                   .HasForeignKey(d => d.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);

        }

        // __ Seed data for DiagnosisType __ //
        private static IEnumerable<DiagnosisType> GetSeedData() =>
        [
            new() { Id=1,  ClinicId=1, Code="CARIES",    NameAr="تسوس",               NameEn="Caries",             CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            new() { Id=2,  ClinicId=1, Code="PULPITIS",  NameAr="التهاب لب",          NameEn="Pulpitis",           CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            new() { Id=3,  ClinicId=1, Code="PERIO_D",   NameAr="أمراض اللثة",        NameEn="Periodontal Disease",CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            new() { Id=4,  ClinicId=1, Code="ABSCESS",   NameAr="خراج",               NameEn="Abscess",            CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            new() { Id=5,  ClinicId=1, Code="FRACTURE",  NameAr="كسر في السن",        NameEn="Tooth Fracture",     CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            new() { Id=6,  ClinicId=1, Code="MISSING",   NameAr="سن مفقود",           NameEn="Missing Tooth",      CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            new() { Id=7,  ClinicId=1, Code="IMPACTED",  NameAr="سن معشم",            NameEn="Impacted Tooth",     CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
            new() { Id=8,  ClinicId=1, Code="ORTHO_N",   NameAr="تقويم مطلوب",        NameEn="Orthodontic Need",   CreatedAt=new DateTime(2024,1,1,0,0,0,DateTimeKind.Utc) },
        ];
    }
}
