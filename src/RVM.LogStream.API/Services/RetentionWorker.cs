using RVM.LogStream.Domain.Interfaces;

namespace RVM.LogStream.API.Services;

public class RetentionWorker(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<RetentionWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("RetentionWorker started");

        var intervalMinutes = configuration.GetValue("Retention:CheckIntervalMinutes", 60);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(TimeSpan.FromMinutes(intervalMinutes), stoppingToken);

                using var scope = scopeFactory.CreateScope();
                var policyRepo = scope.ServiceProvider.GetRequiredService<IRetentionPolicyRepository>();
                var logEntryRepo = scope.ServiceProvider.GetRequiredService<ILogEntryRepository>();

                var policies = await policyRepo.GetEnabledAsync(stoppingToken);

                foreach (var policy in policies)
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    var cutoff = DateTime.UtcNow.AddDays(-policy.RetentionDays);
                    var deleted = await logEntryRepo.DeleteOlderThanAsync(cutoff, policy.SourcePattern, stoppingToken);

                    if (deleted > 0)
                        logger.LogInformation("Retention: purged {Count} entries older than {Days}d for pattern '{Pattern}'",
                            deleted, policy.RetentionDays, policy.SourcePattern);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in RetentionWorker");
            }
        }

        logger.LogInformation("RetentionWorker stopped");
    }
}
