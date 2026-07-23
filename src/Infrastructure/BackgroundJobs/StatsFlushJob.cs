using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProyectoAvengers.Application.Interfaces;

namespace ProyectoAvengers.Infrastructure.BackgroundJobs;

public class StatsFlushJob : BackgroundService
{
    private readonly IViewTracker _viewTracker;
    private readonly ILogger<StatsFlushJob> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);

    public StatsFlushJob(IViewTracker viewTracker, ILogger<StatsFlushJob> logger)
    {
        _viewTracker = viewTracker;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Stats flush job started (interval: {Interval}s)", _interval.TotalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);
                await _viewTracker.FlushAsync(stoppingToken);
                _logger.LogDebug("Stats flushed to database");
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error flushing stats");
            }
        }
    }
}
