using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
            builder.HasKey(p => p.Id);

            // Precision for Amount to ensure it can handle typical currency values
            builder.Property(p => p.Amount)
                   .HasPrecision(10, 2);

            // Store enums as strings for better readability in the database
            builder.Property(p => p.Type)
                   .HasConversion<string>();

            builder.Property(p => p.Status)
                   .HasConversion<string>();

            builder.Property(p => p.PaymentCode)
                   .HasMaxLength(50);

            builder.Property(p => p.GatewayReference)
                   .HasMaxLength(100);

            // Global Query Filter to exclude soft-deleted records
            builder.HasQueryFilter(p => !p.IsDeleted);

            // Relationships
            builder.HasOne(p => p.Patient)
                   .WithMany(pt => pt.Payments)
                   .HasForeignKey(p => p.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
