using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Token)
                   .IsRequired()
                   .HasMaxLength(500);

            builder.Property(r => r.UserId)
                   .IsRequired();

            builder.Property(r => r.ReplacedByToken)
                   .HasMaxLength(500);

            builder.HasIndex(r => r.Token)
                   .IsUnique();

            builder.HasIndex(r => r.UserId);
        }
    }
}
