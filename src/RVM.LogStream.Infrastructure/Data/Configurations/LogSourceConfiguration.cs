using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RVM.LogStream.Domain.Entities;

namespace RVM.LogStream.Infrastructure.Data.Configurations;

public class LogSourceConfiguration : IEntityTypeConfiguration<LogSource>
{
    public void Configure(EntityTypeBuilder<LogSource> builder)
    {
        builder.ToTable("log_sources");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.HasIndex(e => e.LastSeen);

        builder.HasMany(e => e.LogEntries)
            .WithOne()
            .HasForeignKey(e => e.Source)
            .HasPrincipalKey(s => s.Name)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
