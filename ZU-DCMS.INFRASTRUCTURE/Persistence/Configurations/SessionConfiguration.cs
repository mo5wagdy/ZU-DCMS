using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.HasKey(s => s.Id);

            // __ Index on Date for faster queries __ //
            builder.HasIndex(s => s.Date);

            // __ Global Query Filter to exclude soft-deleted __ //
            builder.HasQueryFilter(s => !s.IsDeleted);
        }
    }
}
