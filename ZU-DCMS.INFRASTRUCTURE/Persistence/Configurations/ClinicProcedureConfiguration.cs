using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class ClinicProcedureConfiguration : IEntityTypeConfiguration<ClinicProcedure>
    {
        public void Configure(EntityTypeBuilder<ClinicProcedure> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(x => new { x.ClinicId, x.ProcedureId })
                   .IsUnique();

            builder.HasQueryFilter(x => !x.IsDeleted);

            builder.HasOne(x => x.Clinic)
                   .WithMany(c => c.ProcedureLinks)
                   .HasForeignKey(x => x.ClinicId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.Procedure)
                   .WithMany(p => p.ClinicLinks)
                   .HasForeignKey(x => x.ProcedureId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasData(GetSeedData());
        }

        private static IEnumerable<ClinicProcedure> GetSeedData()
        {
            var dt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return new[]
            {
                // REST (5) — Composite, GIC, Amalgam
                new ClinicProcedure { Id=1,  ClinicId=5, ProcedureId=1,  CreatedAt=dt },
                new ClinicProcedure { Id=2,  ClinicId=5, ProcedureId=2,  CreatedAt=dt },
                new ClinicProcedure { Id=3,  ClinicId=5, ProcedureId=3,  CreatedAt=dt },

                // ENDO (2) — RCT, Pulpectomy, Retreatment
                new ClinicProcedure { Id=4,  ClinicId=2, ProcedureId=4,  CreatedAt=dt },
                new ClinicProcedure { Id=5,  ClinicId=2, ProcedureId=5,  CreatedAt=dt },
                new ClinicProcedure { Id=6,  ClinicId=2, ProcedureId=6,  CreatedAt=dt },

                // SURG (3) — Extraction, Surgical, Drainage, Biopsy
                new ClinicProcedure { Id=7,  ClinicId=3, ProcedureId=7,  CreatedAt=dt },
                new ClinicProcedure { Id=8,  ClinicId=3, ProcedureId=8,  CreatedAt=dt },
                new ClinicProcedure { Id=9,  ClinicId=3, ProcedureId=9,  CreatedAt=dt },
                new ClinicProcedure { Id=10, ClinicId=3, ProcedureId=10, CreatedAt=dt },

                // PERIO (4) — Scaling, Root Planing, Gingivectomy
                new ClinicProcedure { Id=11, ClinicId=4, ProcedureId=11, CreatedAt=dt },
                new ClinicProcedure { Id=12, ClinicId=4, ProcedureId=12, CreatedAt=dt },
                new ClinicProcedure { Id=13, ClinicId=4, ProcedureId=13, CreatedAt=dt },

                // PED (6) — Pulpotomy, SSC Crown, Fluoride, Sealant, Extraction
                new ClinicProcedure { Id=14, ClinicId=6, ProcedureId=14, CreatedAt=dt },
                new ClinicProcedure { Id=15, ClinicId=6, ProcedureId=15, CreatedAt=dt },
                new ClinicProcedure { Id=16, ClinicId=6, ProcedureId=16, CreatedAt=dt },
                new ClinicProcedure { Id=17, ClinicId=6, ProcedureId=17, CreatedAt=dt },
                new ClinicProcedure { Id=18, ClinicId=6, ProcedureId=7,  CreatedAt=dt },

                // FIX (7) — Crown Prep, Bridge, Cementation
                new ClinicProcedure { Id=19, ClinicId=7, ProcedureId=18, CreatedAt=dt },
                new ClinicProcedure { Id=20, ClinicId=7, ProcedureId=19, CreatedAt=dt },
                new ClinicProcedure { Id=21, ClinicId=7, ProcedureId=20, CreatedAt=dt },

                // REM (8) — Complete Denture, Partial Denture, Adjustment
                new ClinicProcedure { Id=22, ClinicId=8, ProcedureId=21, CreatedAt=dt },
                new ClinicProcedure { Id=23, ClinicId=8, ProcedureId=22, CreatedAt=dt },
                new ClinicProcedure { Id=24, ClinicId=8, ProcedureId=23, CreatedAt=dt },
            };
        }
    }
}
