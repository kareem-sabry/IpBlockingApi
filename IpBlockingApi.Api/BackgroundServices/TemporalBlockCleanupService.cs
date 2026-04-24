using IpBlockingApi.Repositories.Interfaces;

namespace IpBlockingApi.BackgroundServices;

/// <summary>
/// A long-running hosted service that periodically scans the in-memory store
/// and removes expired temporal country blocks.
/// Runs on a fixed 5-minute interval as specified by the assignment.
/// </summary>
public sealed class TemporalBlockCleanupService : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(5);

    private readonly ICountryRepository _countryRepo;
    private readonly ILogger<TemporalBlockCleanupService> _logger;

    public TemporalBlockCleanupService(
        ICountryRepository countryRepo,
        ILogger<TemporalBlockCleanupService> logger)
    {
        _countryRepo = countryRepo;
        _logger = logger;
    }

    /// <inheritdoc/>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Temporal block cleanup service started. Interval: {Interval} min.",
            Interval.TotalMinutes);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(Interval, stoppingToken);
                RunCleanup();
            }
            catch (OperationCanceledException)
            {
                // Graceful shutdown — exit the loop without logging an error.
                break;
            }
            catch (Exception ex)
            {
                // Log but keep the service alive so it retries on the next tick.
                _logger.LogError(ex, "Unexpected error during temporal block cleanup.");
            }
        }

        _logger.LogInformation("Temporal block cleanup service stopped.");
    }

    private void RunCleanup()
    {
        var removed = _countryRepo.RemoveExpiredTemporalBlocks();

        if (removed > 0)
            _logger.LogInformation(
                "Cleanup: removed {Count} expired temporal block(s) at {Time:u}.",
                removed, DateTime.UtcNow);
        else
            _logger.LogDebug("Cleanup: no expired temporal blocks found.");
    }
}