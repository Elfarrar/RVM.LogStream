namespace RVM.LogStream.API.Dtos;

public record SearchLogsResponse(
    List<LogEntryResponse> Items,
    int TotalCount,
    int Offset,
    int Limit);

public record LogEntryResponse(
    Guid Id,
    DateTime Timestamp,
    string Level,
    string Message,
    string? MessageTemplate,
    string Source,
    string? CorrelationId,
    string? Properties,
    string? Exception);
