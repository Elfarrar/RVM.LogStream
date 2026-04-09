using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RVM.LogStream.Domain.Entities;

namespace RVM.LogStream.Infrastructure.Data.Configurations;

public class RetentionPolicyConfiguration : IEntityTypeConfiguration<RetentionPolicy>
{
    public void Configure(EntityTypeBuilder<RetentionPolicy> builder)
    {
        builder.ToTable("retention_policies");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SourcePattern).IsRequired().HasMaxLength(200);
        builder.HasIndex(e => e.IsEnabled);
    }
}
