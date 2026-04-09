using RVM.LogStream.Domain.Entities;
using RVM.LogStream.Domain.Enums;

namespace RVM.LogStream.Test.Domain;

public class EntityTests
{
    [Fact]
    public void LogEntry_Defaults_AreCorrect()
    {
        var entry = new LogEntry();

        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.Equal(LogLevel.Trace, entry.Level);
        Assert.Equal(string.Empty, entry.Message);
        Assert.Equal(string.Empty, entry.Source);
        Assert.True(entry.Timestamp <= DateTime.UtcNow);
        Assert.Null(entry.MessageTemplate);
        Assert.Null(entry.CorrelationId);
        Assert.Null(entry.Properties);
        Assert.Null(entry.Exception);
    }

    [Fact]
    public void LogEntry_SetProperties_Persist()
    {
        var ts = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var entry = new LogEntry
        {
            Level = LogLevel.Error,
            Message = "Something failed",
            Source = "my-api",
            Timestamp = ts,
            CorrelationId = "abc-123",
            Properties = """{"key":"value"}""",
            Exception = "NullReferenceException",
        };

        Assert.Equal(LogLevel.Error, entry.Level);
        Assert.Equal("Something failed", entry.Message);
        Assert.Equal("my-api", entry.Source);
        Assert.Equal(ts, entry.Timestamp);
        Assert.Equal("abc-123", entry.CorrelationId);
        Assert.Contains("key", entry.Properties);
        Assert.Equal("NullReferenceException", entry.Exception);
    }

    [Fact]
    public void LogSource_Defaults_AreCorrect()
    {
        var source = new LogSource();

        Assert.NotEqual(Guid.Empty, source.Id);
        Assert.Equal(string.Empty, source.Name);
        Assert.Equal(0, source.TotalCount);
        Assert.True(source.FirstSeen <= DateTime.UtcNow);
        Assert.True(source.LastSeen <= DateTime.UtcNow);
        Assert.Empty(source.LogEntries);
    }

    [Fact]
    public void LogSource_TracksCounts()
    {
        var source = new LogSource { Name = "payments", TotalCount = 100 };
        source.TotalCount += 50;

        Assert.Equal(150, source.TotalCount);
    }

    [Fact]
    public void RetentionPolicy_Defaults_AreCorrect()
    {
        var policy = new RetentionPolicy();

        Assert.NotEqual(Guid.Empty, policy.Id);
        Assert.Equal("*", policy.SourcePattern);
        Assert.Equal(30, policy.RetentionDays);
        Assert.True(policy.IsEnabled);
        Assert.True(policy.CreatedAt <= DateTime.UtcNow);
        Assert.Null(policy.UpdatedAt);
    }

    [Fact]
    public void RetentionPolicy_CustomValues()
    {
        var policy = new RetentionPolicy
        {
            SourcePattern = "my-api",
            RetentionDays = 7,
            IsEnabled = false,
        };

        Assert.Equal("my-api", policy.SourcePattern);
        Assert.Equal(7, policy.RetentionDays);
        Assert.False(policy.IsEnabled);
    }

    [Theory]
    [InlineData(LogLevel.Trace, 0)]
    [InlineData(LogLevel.Debug, 1)]
    [InlineData(LogLevel.Information, 2)]
    [InlineData(LogLevel.Warning, 3)]
    [InlineData(LogLevel.Error, 4)]
    [InlineData(LogLevel.Fatal, 5)]
    public void LogLevel_HasExpectedValues(LogLevel level, int expected)
    {
        Assert.Equal(expected, (int)level);
    }
}
