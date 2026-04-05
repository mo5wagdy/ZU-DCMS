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

            // __ Precision for Amount to ensure it can handle typical currency values __ //
            builder.Property(p => p.Amount)
                   .HasPrecision(10, 2);

            // __ Enum Conversions to string for better readability in the database __ //
            builder.Property(p => p.Type)
                   .HasConversion<string>();

            builder.Property(p => p.Status)
                   .HasConversion<string>();
            //_________________________________________________________________________//

            builder.Property(p => p.PaymentCode)
                   .HasMaxLength(100);

            builder.Property(p => p.GatewayReference)
                   .HasMaxLength(100);

            builder.Property(p => p.GatewayName)
                   .HasMaxLength(50);

            // __ Global Query Filter to exclude soft-deleted records __ //
            builder.HasQueryFilter(p => !p.IsDeleted);

            // ____________ Relationships ____________ //
            builder.HasOne(p => p.Patient)
                   .WithMany(pt => pt.Payments)
                   .HasForeignKey(p => p.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
