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

            // Index on Date for faster queries
            builder.HasIndex(s => s.Date);

            // Global Query Filter to exclude soft-deleted 
            builder.HasQueryFilter(s => !s.IsDeleted);
        }
    }
}
