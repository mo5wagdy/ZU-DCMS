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

            builder.HasIndex(r => r.CaseAssignmentId);
            builder.HasIndex(r => r.TeachingAssistantId);

            builder.Property(x => x.Notes)
                   .HasMaxLength(500);

            builder.Property(x => x.Status)
                   .IsRequired();

            builder.Property(x => x.ReviewedAt)
                   .IsRequired();

            builder.HasQueryFilter(r => !r.IsDeleted);

            // __ Relationships __ //
            builder.HasOne(r => r.CaseAssignment)
                   .WithMany()
                   .HasForeignKey(r => r.CaseAssignmentId)
                   .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasOne(r => r.TeachingAssistant)
                   .WithMany(t => t.CaseReviews)
                   .HasForeignKey(r => r.TeachingAssistantId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
