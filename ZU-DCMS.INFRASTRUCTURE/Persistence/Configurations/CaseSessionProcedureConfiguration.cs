using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class CaseSessionProcedureConfiguration : IEntityTypeConfiguration<CaseSessionProcedure>
    {
        public void Configure(EntityTypeBuilder<CaseSessionProcedure> builder)
        {
            builder.HasKey(x => x.Id);

            // __ A case session cannot have the same procedure assigned more than once __ //
            builder.HasIndex(x => new { x.CaseSessionId, x.ProcedureId })
                   .IsUnique();

            // __ Global Query Filter to exclude soft-deleted records __ //
            builder.HasQueryFilter(x => !x.IsDeleted);

            // ____________ Relationships ____________ //
            builder.HasOne(x => x.Session)
                   .WithMany(s => s.Procedures)
                   .HasForeignKey(x => x.CaseSessionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Procedure)
                   .WithMany(p => p.CaseSessionProcedures)
                   .HasForeignKey(x => x.ProcedureId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
