using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
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

            builder.Property(s => s.Date)
                   .IsRequired()
                   .HasColumnType("date");

            // __ Unique index on Date and StartTime to prevent overlapping sessions on the same day __ //
            builder.HasIndex(s => new { s.Date, s.StartTime }).IsUnique();

            // __ Global Query Filter to exclude soft-deleted __ //
            builder.HasQueryFilter(s => !s.IsDeleted);
        }
    }
}
