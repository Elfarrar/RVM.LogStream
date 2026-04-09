namespace RVM.LogStream.Domain.Entities;

public class RetentionPolicy
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string SourcePattern { get; set; } = "*";
    public int RetentionDays { get; set; } = 30;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
