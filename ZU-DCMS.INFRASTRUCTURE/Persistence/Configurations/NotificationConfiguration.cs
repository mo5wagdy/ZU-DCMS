using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
    {
        public void Configure(EntityTypeBuilder<Notification> builder)
        {
            builder.HasKey(n => n.Id);

            builder.Property(n => n.Title)
                   .IsRequired()
                   .HasMaxLength(200);

            builder.Property(n => n.Message)
                   .IsRequired()
                   .HasMaxLength(1000);

            // Enum Conversions to string for better readability in the database
            builder.Property(n => n.Type)
                   .HasConversion<string>();   

            builder.Property(n => n.Channel)
                   .HasConversion<string>();

            builder.Property(n => n.Status)
                   .HasConversion<string>();

            builder.Property(n => n.ReferenceType)
                   .HasConversion<string>();

            builder.Property(n => n.ReferenceId)
                   .HasMaxLength(50);

            // Index on UserId for faster lookups
            builder.HasIndex(n => n.UserId);

            // Global Query Filter to exclude soft-deleted records
            builder.HasQueryFilter(n => !n.IsDeleted);
        }
    }
}
