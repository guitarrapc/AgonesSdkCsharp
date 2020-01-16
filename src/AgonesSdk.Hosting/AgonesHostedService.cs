using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace AgonesSdk.Hosting
{
    public class AgonesHostedService : BackgroundService
    {
        private readonly IAgonesSdk _agonesSdk;
        private readonly ILogger<AgonesHostedService> _logger;

        public AgonesHostedService(IAgonesSdk agonesSdk, ILogger<AgonesHostedService> logger)
        {
            _agonesSdk = agonesSdk;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogDebug($"{nameof(AgonesHostedService)} Starting Health Ping");
            stoppingToken.Register(() => _logger.LogDebug($" {nameof(AgonesHostedService)} task is stopping"));

            while(!stoppingToken.IsCancellationRequested)
            {
                _logger.LogDebug($"{nameof(AgonesHostedService)} sending Health Ping");
                await _agonesSdk.Health(stoppingToken);
                await Task.Delay(_agonesSdk.Settings.HealthInterval, stoppingToken);
            }

            _logger.LogDebug($"{nameof(AgonesHostedService)} task is stopping.");
        }
    }
}
