using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.INFRASTRUCTURE.Persistence.Configurations
{
    public class CaseReviewConfiguration : IEntityTypeConfiguration<CaseReview>
    {
        public void Configure(EntityTypeBuilder<CaseReview> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Notes)
                   .HasMaxLength(500);

            builder.Property(x => x.Status)
                   .IsRequired();

            builder.Property(x => x.ReviewedAt)
                   .IsRequired();
        }
    }
}
