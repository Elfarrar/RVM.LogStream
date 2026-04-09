namespace RVM.LogStream.Domain.Entities;

public class LogSource
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Name { get; set; } = string.Empty;
    public DateTime FirstSeen { get; set; } = DateTime.UtcNow;
    public DateTime LastSeen { get; set; } = DateTime.UtcNow;
    public long TotalCount { get; set; }

    public ICollection<LogEntry> LogEntries { get; set; } = [];
}
