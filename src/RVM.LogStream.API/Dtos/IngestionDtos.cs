namespace RVM.LogStream.API.Dtos;

public record IngestLogEntryRequest(
    DateTime? Timestamp,
    string Level,
    string Message,
    string? MessageTemplate,
    string Source,
    string? CorrelationId,
    string? Properties,
    string? Exception);

public record IngestBatchRequest(List<IngestLogEntryRequest> Events);

public record IngestBatchResponse(int Accepted, int Rejected);
