using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AgonesSdk.Hosting
{
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

            while(!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug($"{nameof(AgonesHealthCheckService)} sending Health Ping");
                await _agonesSdk.Health(stoppingToken);
                await Task.Delay(_agonesSdk.Settings.HealthInterval, stoppingToken);
            }

            _logger.LogDebug($"{nameof(AgonesHealthCheckService)} task is stopping.");
        }
    }
}
