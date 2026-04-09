namespace RVM.LogStream.API.Dtos;

public record CreateRetentionPolicyRequest(
    string SourcePattern = "*",
    int RetentionDays = 30,
    bool IsEnabled = true);

public record UpdateRetentionPolicyRequest(
    string? SourcePattern,
    int? RetentionDays,
    bool? IsEnabled);

public record RetentionPolicyResponse(
    Guid Id,
    string SourcePattern,
    int RetentionDays,
    bool IsEnabled,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
