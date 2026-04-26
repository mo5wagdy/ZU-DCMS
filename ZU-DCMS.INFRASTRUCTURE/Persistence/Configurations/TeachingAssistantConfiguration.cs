using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ZU_DCMS.Domain.Entities;

namespace ZU_DCMS.Infrastructure.Persistence.Configurations;

public class TeachingAssistantConfiguration : IEntityTypeConfiguration<TeachingAssistant>
{
    public void Configure(EntityTypeBuilder<TeachingAssistant> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.ApplicationUserId)
               .IsRequired();

        builder.Property(t => t.FullName)
               .IsRequired()
               .HasMaxLength(100);

        builder.Property(t => t.TACode)
               .IsRequired()
               .HasMaxLength(20);

        builder.HasIndex(t => t.ApplicationUserId)
               .IsUnique();

        builder.HasIndex(t => t.TACode)
               .IsUnique();

        builder.HasQueryFilter(t => !t.IsDeleted);
    }
}