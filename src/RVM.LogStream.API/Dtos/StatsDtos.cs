namespace RVM.LogStream.API.Dtos;

public record LogStatsResponse(
    long TotalLogs,
    int SourceCount,
    List<LevelCountResponse> ByLevel,
    List<SourceCountResponse> BySource);

public record LevelCountResponse(string Level, int Count);
public record SourceCountResponse(string Source, int Count);
