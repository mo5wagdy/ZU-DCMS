using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class TermConfiguration : IEntityTypeConfiguration<Term>
    {
        public void Configure(EntityTypeBuilder<Term> builder)
        {
            builder.HasKey(t => t.Id);

            builder.HasIndex(t => new { t.Name, t.StartDate })
                   .IsUnique();

            builder.Property(t => t.Name)
                   .IsRequired()
                   .HasMaxLength(100);

            // __ Global Query Filter to exclude soft-deleted records __ //
            builder.HasQueryFilter(t => !t.IsDeleted);
        }
    }
}