using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RVM.LogStream.Domain.Entities;

namespace RVM.LogStream.Infrastructure.Data.Configurations;

public class LogEntryConfiguration : IEntityTypeConfiguration<LogEntry>
{
    public void Configure(EntityTypeBuilder<LogEntry> builder)
    {
        builder.ToTable("log_entries");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Level).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Message).IsRequired().HasMaxLength(8000);
        builder.Property(e => e.MessageTemplate).HasMaxLength(4000);
        builder.Property(e => e.Source).IsRequired().HasMaxLength(200);
        builder.Property(e => e.CorrelationId).HasMaxLength(100);
        builder.Property(e => e.Exception).HasMaxLength(8000);

        builder.HasIndex(e => e.Timestamp).IsDescending();
        builder.HasIndex(e => new { e.Source, e.Timestamp }).IsDescending(false, true);
        builder.HasIndex(e => new { e.Level, e.Timestamp }).IsDescending(false, true);
        builder.HasIndex(e => e.CorrelationId);
    }
}
