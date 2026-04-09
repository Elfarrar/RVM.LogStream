namespace RVM.LogStream.API.Dtos;

public record LogSourceResponse(
    Guid Id,
    string Name,
    DateTime FirstSeen,
    DateTime LastSeen,
    long TotalCount);
