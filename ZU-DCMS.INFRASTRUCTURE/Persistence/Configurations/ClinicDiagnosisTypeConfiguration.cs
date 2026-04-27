using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class ClinicDiagnosisTypeConfiguration : IEntityTypeConfiguration<ClinicDiagnosisType>
    {
        public void Configure(EntityTypeBuilder<ClinicDiagnosisType> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.ClinicId, x.DiagnosisTypeId })
                   .IsUnique();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasOne(x => x.Clinic)
                   .WithMany(c => c.DiagnosisTypeLinks)
                   .HasForeignKey(x => x.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.DiagnosisType)
                   .WithMany(d => d.ClinicLinks)
                   .HasForeignKey(x => x.DiagnosisTypeId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(GetSeedData());
        }

        private static IEnumerable<ClinicDiagnosisType> GetSeedData()
        {
            var dt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return new[]
            {
                // DIAG (1)
                new ClinicDiagnosisType { Id=1,  ClinicId=1, DiagnosisTypeId=1,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=2,  ClinicId=1, DiagnosisTypeId=2,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=3,  ClinicId=1, DiagnosisTypeId=3,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=4,  ClinicId=1, DiagnosisTypeId=4,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=5,  ClinicId=1, DiagnosisTypeId=5,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=6,  ClinicId=1, DiagnosisTypeId=6,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=7,  ClinicId=1, DiagnosisTypeId=7,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=8,  ClinicId=1, DiagnosisTypeId=8,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=9,  ClinicId=1, DiagnosisTypeId=9,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=10, ClinicId=1, DiagnosisTypeId=10, CreatedAt=dt },
                new ClinicDiagnosisType { Id=11, ClinicId=1, DiagnosisTypeId=11, CreatedAt=dt },
                new ClinicDiagnosisType { Id=12, ClinicId=1, DiagnosisTypeId=12, CreatedAt=dt },
                new ClinicDiagnosisType { Id=13, ClinicId=1, DiagnosisTypeId=13, CreatedAt=dt },
                new ClinicDiagnosisType { Id=14, ClinicId=1, DiagnosisTypeId=14, CreatedAt=dt },
                new ClinicDiagnosisType { Id=15, ClinicId=1, DiagnosisTypeId=15, CreatedAt=dt },

                // ENDO (2) — Pulpitis, Pulp Necrosis, Periapical Abscess
                new ClinicDiagnosisType { Id=16, ClinicId=2, DiagnosisTypeId=2,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=17, ClinicId=2, DiagnosisTypeId=3,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=18, ClinicId=2, DiagnosisTypeId=4,  CreatedAt=dt },

                // SURG (3) — Impacted, Root Remnants, Abscess, Fracture
                new ClinicDiagnosisType { Id=19, ClinicId=3, DiagnosisTypeId=7,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=20, ClinicId=3, DiagnosisTypeId=8,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=21, ClinicId=3, DiagnosisTypeId=9,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=22, ClinicId=3, DiagnosisTypeId=10, CreatedAt=dt },

                // PERIO (4) — Gingivitis, Periodontitis, Hyperplasia, Mobility
                new ClinicDiagnosisType { Id=23, ClinicId=4, DiagnosisTypeId=5,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=24, ClinicId=4, DiagnosisTypeId=6,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=25, ClinicId=4, DiagnosisTypeId=14, CreatedAt=dt },
                new ClinicDiagnosisType { Id=26, ClinicId=4, DiagnosisTypeId=15, CreatedAt=dt },

                // REST (5) — Caries, Fracture
                new ClinicDiagnosisType { Id=27, ClinicId=5, DiagnosisTypeId=1,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=28, ClinicId=5, DiagnosisTypeId=10, CreatedAt=dt },

                // PED (6) — Early Childhood Caries, Caries, Impacted
                new ClinicDiagnosisType { Id=29, ClinicId=6, DiagnosisTypeId=13, CreatedAt=dt },
                new ClinicDiagnosisType { Id=30, ClinicId=6, DiagnosisTypeId=1,  CreatedAt=dt },
                new ClinicDiagnosisType { Id=31, ClinicId=6, DiagnosisTypeId=7,  CreatedAt=dt },

                // FIX (7) — Missing Tooth, Fracture
                new ClinicDiagnosisType { Id=32, ClinicId=7, DiagnosisTypeId=11, CreatedAt=dt },
                new ClinicDiagnosisType { Id=33, ClinicId=7, DiagnosisTypeId=10, CreatedAt=dt },

                // REM (8) — Missing Tooth
                new ClinicDiagnosisType { Id=34, ClinicId=8, DiagnosisTypeId=11, CreatedAt=dt },
            };
        }
    }
}
