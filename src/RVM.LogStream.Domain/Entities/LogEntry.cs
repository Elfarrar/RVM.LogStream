using RVM.LogStream.Domain.Enums;

namespace RVM.LogStream.Domain.Entities;

public class LogEntry
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public LogLevel Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? MessageTemplate { get; set; }
    public string Source { get; set; } = string.Empty;
    public string? CorrelationId { get; set; }
    public string? Properties { get; set; }
    public string? Exception { get; set; }
}
