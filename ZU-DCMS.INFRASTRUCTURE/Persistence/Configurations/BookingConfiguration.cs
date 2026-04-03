using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(b => b.Id);

            builder.HasIndex(b => b.BookingCode)
                   .IsUnique();
            
            builder.Property(b => b.BookingCode)
                   .IsRequired()
                   .HasMaxLength(20);

            // Enum Conversions to string for better readability in the database
            builder.Property(b => b.BookingType)
                   .HasConversion<string>();

            builder.Property(b => b.Status)
                   .HasConversion<string>();

            builder.Property(b => b.PreliminaryComplaint)
                   .HasMaxLength(500);

            builder.Property(b => b.PostponeReason)
                   .HasMaxLength(500);

            // Global Query Filter to exclude soft-deleted records
            builder.HasQueryFilter(b => !b.IsDeleted);

            // Relationships
            builder.HasOne(b => b.Patient)
                   .WithMany(p => p.Bookings)
                   .HasForeignKey(b => b.PatientId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Session)
                   .WithMany(s => s.Bookings)
                   .HasForeignKey(b => b.SessionId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.Payment)
                   .WithOne(p => p.Booking)
                   .HasForeignKey<Payment>(p => p.BookingId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(b => b.DiagnosisRecord)
                   .WithOne(d => d.Booking)
                   .HasForeignKey<DiagnosisRecord>(d => d.BookingId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
