using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Polly;
using System;
using System.Net.Http.Headers;

namespace AgonesSdk.Hosting
{
    public static class AgonesSdkService
    {
        private static readonly Lazy<Random> jitterer = new Lazy<Random>(() => new Random());

        /// <summary>
        /// Add AgonesSdk and run Health Check in the background.
        /// </summary>
        /// <remarks>You can retrieve IAgonesSdk and AgonesSdkSettings through DI</remarks>
        /// <param name="hostBuilder"></param>
        /// <param name="registerHealthCheckService"></param>
        /// <returns></returns>
        public static IHostBuilder AddAgones(this IHostBuilder hostBuilder, bool registerHealthCheckService = true)
            => hostBuilder.AddAgones(new AgonesSdkOptions(), registerHealthCheckService);
        /// <summary>
        /// Add AgonesSdk and run Health Check in the background.
        /// </summary>
        /// <remarks>You can retrieve IAgonesSdk and AgonesSdkSettings through DI</remarks>
        /// <param name="hostBuilder"></param>
        /// <param name="settings"></param>
        /// <param name="registerHealthCheckService"></param>
        /// <returns></returns>
        public static IHostBuilder AddAgones(this IHostBuilder hostBuilder, AgonesSdkOptions settings, bool registerHealthCheckService = true)
        {
            return hostBuilder.ConfigureServices((hostContext, services) =>
            {
                ConfigureAgonesService(services, settings, ConfigureHttpClientDefault, registerHealthCheckService);
            });

            static void ConfigureHttpClientDefault(IServiceCollection services, AgonesSdkOptions settings)
            {
                services.AddHttpClient(settings.HttpClientName, client => 
                {
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Add("User-Agent", settings.HttpClientUserAgent);
                })
                .AddTransientHttpErrorPolicy(x => x.WaitAndRetryAsync(settings.PollyOptions.FailedRetryCount, retry => ExponentialBackkoff(retry)))
                .AddTransientHttpErrorPolicy(x => x.CircuitBreakerAsync(settings.PollyOptions.HandledEventsAllowedBeforeCirtcuitBreaking, settings.PollyOptions.CirtcuitBreakingDuration));
            }
        }
        /// <summary>
        /// Add AgonesSdk and run Health Check in the background.
        /// </summary>
        /// <param name="hostBuilder"></param>
        /// <param name="settings"></param>
        /// <param name="configureHttpClient"></param>
        /// <param name="registerHealthCheckService"></param>
        /// <returns></returns>
        public static IHostBuilder AddAgones(this IHostBuilder hostBuilder, AgonesSdkOptions settings, Action<IServiceCollection, AgonesSdkOptions> configureHttpClient, bool registerHealthCheckService = true) => hostBuilder.ConfigureServices((hostContext, services)
            => hostBuilder.ConfigureServices((hostContext, services) => ConfigureAgonesService(services, settings, configureHttpClient, registerHealthCheckService)));

        private static void ConfigureAgonesService(IServiceCollection services, AgonesSdkOptions settings, Action<IServiceCollection, AgonesSdkOptions> configureHttpClient, bool registerHostedService)
        {
            configureHttpClient.Invoke(services, settings);
            services.AddSingleton<AgonesSdkOptions>(settings);
            services.AddSingleton<IAgonesSdk, AgonesSdk>();

            if (registerHostedService)
            {
                services.AddHostedService<AgonesHealthCheckService>();
            }
        }

        /// <summary>
        /// Provide Exponential Backoff
        /// </summary>
        /// <param name="retryAttempt"></param>
        /// <returns></returns>
        public static TimeSpan ExponentialBackkoff(int retryAttempt)
            => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) + TimeSpan.FromMilliseconds(jitterer.Value.Next(0, 100));
    }
}
