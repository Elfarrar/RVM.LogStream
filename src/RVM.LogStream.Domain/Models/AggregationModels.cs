using RVM.LogStream.Domain.Enums;

namespace RVM.LogStream.Domain.Models;

public record LogVolumeByLevel(LogLevel Level, int Count);
public record LogVolumeBySource(string Source, int Count);
