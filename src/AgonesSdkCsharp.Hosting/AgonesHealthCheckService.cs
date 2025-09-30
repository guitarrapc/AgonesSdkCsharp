using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace AgonesSdkCsharp.Hosting;

/// <summary>
/// Background service to execute HealthCheck to the AgonesSdk
/// </summary>
public class AgonesHealthCheckService : BackgroundService
{
    private readonly IAgonesSdk _agonesSdk;
    private readonly ILogger<AgonesHealthCheckService> _logger;

    public AgonesHealthCheckService(IAgonesSdk agonesSdk, ILogger<AgonesHealthCheckService> logger)
    {
        _agonesSdk = agonesSdk;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogDebug($"{nameof(AgonesHealthCheckService)} Starting Health Ping");
        stoppingToken.Register(() => _logger.LogDebug($" {nameof(AgonesHealthCheckService)} task is stopping"));

        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogDebug($"{nameof(AgonesHealthCheckService)} sending Health Ping");
            _agonesSdk.Health(stoppingToken).FireAndForget(_logger);
            await Task.Delay(_agonesSdk.Options.HealthInterval, stoppingToken);
        }

        _logger.LogDebug($"{nameof(AgonesHealthCheckService)} task is stopping.");
    }
}
